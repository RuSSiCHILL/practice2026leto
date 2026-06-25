using System;
using System.Collections.Generic;
using System.Linq;

namespace task02
{
public class Student
    {
        public string? Name { get; set; }
        public string? Faculty { get; set; }
        public List<int>? Grades { get; set; }
    }
public class StudentService
{
    private readonly List<Student> _students;

    public StudentService(List<Student> students) => _students = students;


    // 1. Возвращает студентов указанного факультета
    public IEnumerable<Student> GetStudentsByFaculty(string faculty)
    {
        return _students.Where(s => s.Faculty==faculty);
    }


    // 2. Возвращает студентов со средним баллом >= minAverageGrade
    public IEnumerable<Student> GetStudentsWithMinAverageGrade(double minAverageGrade)
    {
        return _students.Where(s => s.Grades!=null && s.Grades.Average()>=minAverageGrade);
    }

    // 3. Возвращает студентов, отсортированных по имени (A-Z)
    public IEnumerable<Student> GetStudentsOrderedByName()
    {
        return _students.OrderBy(s => s.Name);
    }

    // 4. Группировка по факультету
    public ILookup<string, Student> GroupStudentsByFaculty()
    {
        return _students.ToLookup(s => s.Faculty);
    }

    // 5. Находит факультет с максимальным средним баллом
    public string GetFacultyWithHighestAverageGrade()
    {
        return _students
        .GroupBy(s => s.Faculty)
        .Select(g => new
        { 
            Faculty = g.Key, Average=g.Average(s => s.Grades.Average())
        })
        .OrderByDescending(a => a.Average)
        .First().Faculty;
    }
}
}