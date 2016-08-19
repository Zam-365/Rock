﻿// <copyright>
// Copyright by the Spark Development Network
//
// Licensed under the Rock Community License (the "License");
// you may not use this file except in compliance with the License.
// You may obtain a copy of the License at
//
// http://www.rockrms.com/license
//
// Unless required by applicable law or agreed to in writing, software
// distributed under the License is distributed on an "AS IS" BASIS,
// WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
// See the License for the specific language governing permissions and
// limitations under the License.
// </copyright>
//
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Linq;
using System.Web.UI;
using System.Web.UI.WebControls;
using Rock;
using Rock.Attribute;
using Rock.Data;
using Rock.Model;
using Rock.Web.Cache;
using Rock.Web.UI;
using Rock.Web.UI.Controls;

namespace RockWeb.Plugins.com_centralaz.CheckIn
{
    /// <summary>
    /// 
    /// </summary>
    [DisplayName( "Checkin Summary" )]
    [Category( "com_centralaz > Check-in" )]
    [Description( "Helps to view active checkin groups and their locations." )]
    [AttributeField( Rock.SystemGuid.EntityType.GROUP, "Group Age Range Attribute", "Select the attribute used to define the age range of the group", true, false, "43511B8F-71D9-423A-85BF-D1CD08C1998E", order: 2 )]
    [AttributeField( Rock.SystemGuid.EntityType.GROUP, "Group Special Needs Attribute", "Select the attribute used to filter special needs groups.", true, false, "9210EC95-7B85-4D11-A82E-0B677B32704E", order: 2 )]

    public partial class CheckinSummary : RockBlock
    {
        String AgeRangeAttributeKey
        {
            get
            {
                // get the admin-selected attribute key instead of using a hardcoded key
                var ageRangeAttributeKey = string.Empty;
                var ageRangeAttributeGuid = GetAttributeValue( "GroupAgeRangeAttribute" ).AsGuid();
                if ( ageRangeAttributeGuid != Guid.Empty )
                {
                    ageRangeAttributeKey = AttributeCache.Read( ageRangeAttributeGuid ).Key;
                }
                return ageRangeAttributeKey;
            }
        }

        String SpecialNeedsKey
        {
            get
            {
                // get the admin-selected attribute key instead of using a hardcoded key
                var groupSpecialNeedsKey = string.Empty;
                var groupSpecialNeedsGuid = GetAttributeValue( "GroupSpecialNeedsAttribute" ).AsGuid();
                if ( groupSpecialNeedsGuid != Guid.Empty )
                {
                    var groupSpecialNeeds = AttributeCache.Read( groupSpecialNeedsGuid );
                    if ( groupSpecialNeeds != null )
                    {
                        groupSpecialNeedsKey = AttributeCache.Read( groupSpecialNeedsGuid ).Key;
                    }
                }
                return groupSpecialNeedsKey;
            }
        }
        #region Control Methods

        /// <summary>
        /// Raises the <see cref="E:System.Web.UI.Control.Init" /> event.
        /// </summary>
        /// <param name="e">An <see cref="T:System.EventArgs" /> object that contains the event data.</param>
        protected override void OnInit( EventArgs e )
        {
            base.OnInit( e );

            pCategory.EntityTypeId = EntityTypeCache.GetId( typeof( Rock.Model.Schedule ) ) ?? 0;

            rFilter.ApplyFilterClick += rFilter_ApplyFilterClick;
            rFilter.DisplayFilterValue += rFilter_DisplayFilterValue;
            BindFilter();

            gGroupLocations.DataKeyNames = new string[] { "Id" };
            gGroupLocations.Actions.ShowAdd = false;
            gGroupLocations.IsDeleteEnabled = false;
            gGroupLocations.GridRebind += gGroupLocations_GridRebind;
        }

        /// <summary>
        /// Raises the <see cref="E:System.Web.UI.Control.Load" /> event.
        /// </summary>
        /// <param name="e">The <see cref="T:System.EventArgs" /> object that contains the event data.</param>
        protected override void OnLoad( EventArgs e )
        {
            if ( !Page.IsPostBack )
            {
                pCategory.SetValue( rFilter.GetUserPreference( "Category" ).AsInteger() );
                LoadSchedules();

                BindGrid();
            }

            base.OnLoad( e );
        }

        #endregion

        #region Grid Filter

        /// <summary>
        /// Binds any needed data to the Grid Filter also using the user's stored
        /// preferences.
        /// </summary>
        private void BindFilter()
        {
            ddlGroupType.Items.Clear();
            ddlGroupType.Items.Add( Rock.Constants.All.ListItem );

            // populate the GroupType DropDownList only with GroupTypes with GroupTypePurpose of Checkin Template
            int groupTypePurposeCheckInTemplateId = DefinedValueCache.Read( new Guid( Rock.SystemGuid.DefinedValue.GROUPTYPE_PURPOSE_CHECKIN_TEMPLATE ) ).Id;

            var rockContext = new RockContext();

            GroupTypeService groupTypeService = new GroupTypeService( rockContext );
            var groupTypeList = groupTypeService.Queryable()
                .Where( a => a.GroupTypePurposeValueId == groupTypePurposeCheckInTemplateId )
                .ToList();
            foreach ( var groupType in groupTypeList )
            {
                ddlGroupType.Items.Add( new ListItem( groupType.Name, groupType.Id.ToString() ) );
            }

            ddlGroupType.SetValue( rFilter.GetUserPreference( "Group Type" ) );

            // hide the GroupType filter if this page has a groupTypeId parameter
            int? groupTypeIdPageParam = this.PageParameter( "groupTypeId" ).AsIntegerOrNull();
            if ( groupTypeIdPageParam.HasValue )
            {
                ddlGroupType.Visible = false;
            }

            int? categoryId = rFilter.GetUserPreference( "Category" ).AsIntegerOrNull();
            if ( !categoryId.HasValue )
            {
                var categoryCache = CategoryCache.Read( Rock.SystemGuid.Category.SCHEDULE_SERVICE_TIMES.AsGuid() );
                categoryId = categoryCache != null ? categoryCache.Id : (int?)null;
            }

            if ( categoryId.HasValue )
            {
                pCategory.SetValue( new CategoryService( rockContext ).Get( categoryId.Value ) );
            }
            else
            {
                pCategory.SetValue( null );
            }

        }

        #endregion

        #region Grid/Filter Events

        /// <summary>
        /// Handles the ApplyFilterClick event of the rFilter control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="EventArgs"/> instance containing the event data.</param>
        private void rFilter_ApplyFilterClick( object sender, EventArgs e )
        {
            rFilter.SaveUserPreference( "Group Type", ddlGroupType.SelectedValueAsId().ToString() );
            BindGrid();
        }

        /// <summary>
        /// Rs the filter_ display filter value.
        /// </summary>
        /// <param name="sender">The sender.</param>
        /// <param name="e">The e.</param>
        private void rFilter_DisplayFilterValue( object sender, Rock.Web.UI.Controls.GridFilter.DisplayFilterValueArgs e )
        {
            int itemId = e.Value.AsInteger();
            switch ( e.Key )
            {
                case "Group Type":

                    int? groupTypeIdPageParam = this.PageParameter( "groupTypeId" ).AsIntegerOrNull();

                    //// we only use the GroupType from the filter in cases where there isn't a PageParam of groupTypeId
                    // but just in case the filter wants to display the GroupName, override the itemId with the groupTypeId PageParam
                    if ( groupTypeIdPageParam.HasValue )
                    {
                        itemId = groupTypeIdPageParam.Value;
                    }

                    var groupType = GroupTypeCache.Read( itemId );
                    if ( groupType != null )
                    {
                        e.Value = groupType.Name;
                    }
                    else
                    {
                        e.Value = Rock.Constants.All.Text;
                    }

                    break;

                case "Category":

                    // even though it is technically a filter, don't show it as a filter since we don't show category in the filter UI
                    e.Value = null;

                    break;

                case "Schedules":
                    var schedule = new ScheduleService( new RockContext() ).Get( itemId );
                    if ( schedule != null )
                    {
                        e.Value = schedule.Name;
                    }
                    else
                    {
                        e.Value = Rock.Constants.None.Text;
                    }

                    break;

                case "Parent Location":

                    var location = new LocationService( new RockContext() ).Get( itemId );
                    if ( location != null )
                    {
                        e.Value = location.Name;
                    }
                    else
                    {
                        e.Value = Rock.Constants.All.Text;
                    }

                    break;
            }
        }

        /// <summary>
        /// Handles the GridRebind event of the gGroupLocations control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="EventArgs"/> instance containing the event data.</param>
        private void gGroupLocations_GridRebind( object sender, EventArgs e )
        {
            BindGrid();
        }

        /// <summary>
        /// Binds the grid.
        /// </summary>
        protected void BindGrid()
        {
            var rockContext = new RockContext();
            var groupTypeService = new GroupTypeService( rockContext );
            var groupService = new GroupService( rockContext );

            var groupQry = groupService.Queryable();
            int groupTypeId;

            // if this page has a PageParam for groupTypeId use that to limit which groupTypeId to see. Otherwise, use the groupTypeId specified in the filter
            int? groupTypeIdPageParam = this.PageParameter( "groupTypeId" ).AsIntegerOrNull();
            if ( groupTypeIdPageParam.HasValue )
            {
                groupTypeId = groupTypeIdPageParam ?? Rock.Constants.All.Id;
            }
            else
            {
                groupTypeId = ddlGroupType.SelectedValueAsInt() ?? Rock.Constants.All.Id;
            }

            if ( groupTypeId != Rock.Constants.All.Id )
            {
                var descendantGroupTypeIds = groupTypeService.GetAllAssociatedDescendents( groupTypeId ).Select( a => a.Id );

                // filter to groups that either are of the GroupType or are of a GroupType that has the selected GroupType as a parent (ancestor)
                groupQry = groupQry.Where( a => a.GroupType.Id == groupTypeId || descendantGroupTypeIds.Contains( a.GroupTypeId ) );
            }
            else
            {
                // if no specific GroupType is specified, show all GroupTypes with GroupTypePurpose of Checkin Template and their descendents (since this blocktype is specifically for Checkin)
                int groupTypePurposeCheckInTemplateId = DefinedValueCache.Read( new Guid( Rock.SystemGuid.DefinedValue.GROUPTYPE_PURPOSE_CHECKIN_TEMPLATE ) ).Id;
                List<int> descendantGroupTypeIds = new List<int>();
                foreach ( var templateGroupType in groupTypeService.Queryable().Where( a => a.GroupTypePurposeValueId == groupTypePurposeCheckInTemplateId ) )
                {
                    foreach ( var childGroupType in groupTypeService.GetChildGroupTypes( templateGroupType.Id ) )
                    {
                        descendantGroupTypeIds.Add( childGroupType.Id );
                        descendantGroupTypeIds.AddRange( groupTypeService.GetAllAssociatedDescendents( childGroupType.Id ).Select( a => a.Id ).ToList() );
                    }
                }

                groupQry = groupQry.Where( a => descendantGroupTypeIds.Contains( a.GroupTypeId ) );
            }


            var selectedServiceTimes = cblSchedules.SelectedValuesAsInt;
            groupQry = groupQry.Where( g => g.GroupLocations.Any( gl => gl.Schedules.Any( s => selectedServiceTimes.Contains( s.Id ) ) ) );

            if ( gGroupLocations.SortProperty != null )
            {
                groupQry = groupQry.Sort( gGroupLocations.SortProperty );
            }
            else
            {
                groupQry = groupQry.OrderBy( a => a.Order );
            }

            var qryList = groupQry.ToList();
            gGroupLocations.DataSource = qryList;
            gGroupLocations.DataBind();
        }

        #endregion        

        /// <summary>
        /// Handles the SelectItem event of the pCategory control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="EventArgs"/> instance containing the event data.</param>
        protected void pCategory_SelectItem( object sender, EventArgs e )
        {
            rFilter.SaveUserPreference( "Category", pCategory.SelectedValueAsId().ToString() );
            LoadSchedules();
            BindGrid();
        }

        private void LoadSchedules()
        {
            cblSchedules.Items.Clear();
            if ( pCategory.SelectedValueAsId() != null )
            {
                var selectedCategory = pCategory.SelectedValueAsId().Value;
                cblSchedules.DataSource = new ScheduleService( new RockContext() ).Queryable().Where( s => s.CategoryId == selectedCategory ).ToList();
                cblSchedules.DataBind();
                var selectedItems = rFilter.GetUserPreference( "Schedules" ).SplitDelimitedValues();
                foreach ( ListItem li in cblSchedules.Items )
                {
                    if ( selectedItems.Contains( li.Value ) )
                    {
                        li.Selected = true;
                    }
                }
            }
        }

        protected void cblSchedules_SelectedIndexChanged( object sender, EventArgs e )
        {
            rFilter.SaveUserPreference( "Schedules", cblSchedules.SelectedValues.AsDelimited( "," ) );
            BindGrid();
        }

        protected void gGroupLocations_RowDataBound( object sender, GridViewRowEventArgs e )
        {
            if ( e.Row.DataItem != null )
            {
                var group = e.Row.DataItem as Group;
                group.LoadAttributes();
                Literal lGroupName = e.Row.FindControl( "lGroupName" ) as Literal;
                if ( lGroupName != null )
                {
                    lGroupName.Text = group.Name;
                }

                Literal lAbilityLevel = e.Row.FindControl( "lAbilityLevel" ) as Literal;
                if ( lAbilityLevel != null )
                {
                    lAbilityLevel.Text = group.GetAttributeValues( "AbilityLevel" ).Select( a => DefinedValueCache.Read( a ).Value ).ToList().AsDelimited( ", ", null );
                }

                Literal lAge = e.Row.FindControl( "lAge" ) as Literal;
                if ( lAge != null )
                {
                    lAge.Text = group.GetAttributeValue( AgeRangeAttributeKey ).SplitDelimitedValues().Select( a => a.AsDouble().ToString( "0.##" ) )
                        .ToList().AsDelimited( " to " );
                }

                Literal lGrade = e.Row.FindControl( "lGrade" ) as Literal;
                if ( lGrade != null )
                {
                    var grades = group.GetAttributeValue( "GradeRange" ).SplitDelimitedValues().Select( v => DefinedValueCache.Read( v ).Value.AsInteger() ).ToList();
                    List<String> gradesFormatted = new List<string>();
                    foreach ( var grade in grades )
                    {
                        if ( grade >= 0 )
                        {
                            var schoolGrades = DefinedTypeCache.Read( Rock.SystemGuid.DefinedType.SCHOOL_GRADES.AsGuid() );
                            if ( schoolGrades != null )
                            {
                                var sortedGradeValues = schoolGrades.DefinedValues.OrderBy( a => a.Value.AsInteger() );
                                var schoolGradeValue = sortedGradeValues.Where( a => a.Value.AsInteger() >= grade ).FirstOrDefault();
                                if ( schoolGradeValue != null )
                                {
                                    gradesFormatted.Add( schoolGradeValue.Description );
                                }
                            }
                        }
                    }
                    lGrade.Text = gradesFormatted.AsDelimited( " to " );
                }

                Literal lSpecialNeeds = e.Row.FindControl( "lSpecialNeeds" ) as Literal;
                if ( lSpecialNeeds != null )
                {
                    if ( group.GetAttributeValue( SpecialNeedsKey ).AsBoolean() )
                    {
                        lSpecialNeeds.Text = "<i class='fa fa-check'/>";
                    }
                }

                Literal lLastName = e.Row.FindControl( "lLastName" ) as Literal;
                if ( lLastName != null )
                {
                    string lastNameBeginLetterRange = group.GetAttributeValue( "LastNameBeginLetterRange" ) != null ? group.GetAttributeValue( "LastNameBeginLetterRange" ).Trim() : "";
                    string lastNameEndLetterRange = group.GetAttributeValue( "LastNameEndLetterRange" ) != null ? group.GetAttributeValue( "LastNameEndLetterRange" ).Trim() : "";
                    char rangeStart = ( lastNameBeginLetterRange == "" ) ? 'A' : char.Parse( lastNameBeginLetterRange.ToUpper() );
                    char rangeEnd = ( lastNameEndLetterRange == "" ) ? 'Z' : char.Parse( lastNameEndLetterRange.ToUpper() );
                    lLastName.Text = String.Format( "{0} to {1}", rangeStart, rangeEnd );
                }

                Literal lLocations = e.Row.FindControl( "lLocations" ) as Literal;
                var selectedServiceTimes = cblSchedules.SelectedValuesAsInt;
                if ( lLocations != null )
                {
                    var locations = group.GroupLocations.Where( gl => gl.Schedules.Any( s => selectedServiceTimes.Contains( s.Id ) ) );
                    List<String> locationsFormatted = new List<string>();
                    foreach ( var location in locations )
                    {
                        if ( location.Location.IsActive )
                        {
                            locationsFormatted.Add( location.Location.Name );
                        }
                        else
                        {
                            locationsFormatted.Add( String.Format( "<del style='text-decoration:line-through;color:red;' title='closed'><span style='color:#000'>{0}</span></del>", location.Location.Name ) );
                        }
                    }

                    lLocations.Text = locationsFormatted.AsDelimited( ", ", null );
                }

                Literal lGroup = e.Row.FindControl( "lGroup" ) as Literal;
                if ( lGroup != null )
                {
                    lGroup.Text = group.GroupType.AttendanceRule.ToString();
                }
            }
        }
    }
}