// <copyright>
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

using System.Collections.Generic;
using Rock.Blocks;
using Rock.Mobile;
using Rock.Obsidian.Util;

namespace Rock.Obsidian.Blocks
{
    /// <summary>
    /// Obsidian Block Type
    /// </summary>
    /// <seealso cref="Rock.Blocks.RockBlockType" />
    /// <seealso cref="IObsidianBlockType" />
    public abstract class ObsidianBlockType : RockBlockType, IObsidianBlockType
    {
        #region Properties

        /// <summary>
        /// Gets the required Obsidian interface version.
        /// </summary>
        /// <value>
        /// The required obsidian interface version.
        /// </value>
        public abstract int RequiredObsidianVersion { get; }

        /// <summary>
        /// Gets the block markup file identifier.
        /// </summary>
        /// <value>
        /// The block markup file identifier.
        /// </value>
        public abstract string BlockMarkupFileIdentifier { get; }

        #endregion

        #region Methods

        /// <summary>
        /// Gets the property values that will be sent to the browser.
        /// </summary>
        /// <returns>
        /// A collection of string/object pairs.
        /// </returns>
        public virtual object GetConfigurationValues() {
            return null;
        }

        /// <summary>
        /// Renders the control.
        /// </summary>
        /// <returns></returns>
        public override string GetControlMarkup()
        {
            var rootElementId = $"obsidian-{BlockCache.Guid}";

            return
$@"<div id=""{rootElementId}""></div>
<script type=""text/javascript"">
(function () {{
    Obsidian.initializeBlock({{
        blockFileIdentifier: '{BlockMarkupFileIdentifier}',
        rootElement: document.getElementById('{rootElementId}'),
        pageGuid: '{BlockCache.Page.Guid}',
        blockGuid: '{BlockCache.Guid}',
        configurationValues: {JavaScript.ToJavaScriptObject( GetConfigurationValues() )},
    }});
}})();
</script>";
        }

        #endregion Methods
    }
}
