﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Rnx.TaskLoader
{
    /// <summary>
    /// Responsible for loading all types that provide Rnx tasks.
    /// </summary>
    public interface ITaskTypeLoader
    {
        /// <summary>
        /// Returns all types that contain configurations for Rnx tasks
        /// </summary>
        /// <param name="searchPatterns">The globb searchPatterns where the type loader is looking for informations on what types to load</param>
        IEnumerable<Type> Load(string baseDirectory, params string[] searchPatterns);
    }
}