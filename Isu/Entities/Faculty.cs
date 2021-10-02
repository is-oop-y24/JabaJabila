﻿using System;
using System.Collections.Generic;
using Isu.DataTypes;

namespace Isu.Entities
{
    public class Faculty
    {
        private readonly List<Course> _courses;

        internal Faculty(string name, char letterOfFaculty, MegaFaculty megaFaculty)
        {
            Name = name ?? throw new ArgumentNullException(
                nameof(name),
                $"{nameof(name)} can't be null!");

            Letter = letterOfFaculty;
            MegaFaculty = megaFaculty ?? throw new ArgumentNullException(
                nameof(megaFaculty),
                $"{nameof(megaFaculty)} can't be null!");

            _courses = new List<Course>();
        }

        public string Name { get; }
        public char Letter { get; }
        public IReadOnlyCollection<Course> Courses => _courses;
        public MegaFaculty MegaFaculty { get; }

        internal Course AddCourse(CourseNumber number)
        {
            var course = new Course(number, this);
            _courses.Add(course);
            return course;
        }
    }
}