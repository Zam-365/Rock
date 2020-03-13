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

using Rock.Blocks;

namespace Rock.Obsidian.Blocks
{
    /// <summary>
    /// Defines the properties and methods that all Obsidian blocks must implement.
    /// </summary>
    /// <seealso cref="IRockBlockType" />
    public interface IObsidianBlockType : IRockBlockType
    {
        /// <summary>
        /// Gets the required Obsidian interface version.
        /// </summary>
        /// <value>
        /// The required obsidian interface version.
        /// </value>
        int RequiredObsidianVersion { get; }

        /// <summary>
        /// Gets the block markup file identifier.
        /// </summary>
        /// <value>
        /// The block markup file identifier.
        /// </value>
        string BlockMarkupFileIdentifier { get; }

        /// <summary>
        /// Gets the property values that will be sent to the block.
        /// </summary>
        /// <returns>A collection of string/object pairs.</returns>
        object GetConfigurationValues();
    }
}
