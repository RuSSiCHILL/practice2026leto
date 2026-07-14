using System;
using System.Collections.Generic;
using System.IO;
using Xunit;

public class StudentSerializerTests
{
    private readonly Student _student = new()
    {
        FirstName = "Иван",
        LastName = "Иванов",
        BirthDate = new DateTime(2000, 5, 15),
        Grades = new List<Subject>
        {
            new() { Name = "Математика", Grade = 5 },
            new() { Name = "Физика", Grade = 4 }
        }
    };

    [Fact]
    public void Serialize_ContainsData()
    {
        var json = StudentSerializer.Serialize(_student);
        Assert.Contains("Иван", json);
        Assert.Contains("15.05.2000", json);
    }

    [Fact]
    public void Serialize_UsesCustomDateFormat()
    {
        var json = StudentSerializer.Serialize(_student);
        Assert.Contains("15.05.2000", json);
        Assert.DoesNotContain("2000-05-15", json);
    }

    [Fact]
    public void Deserialize_RestoresObject()
    {
        var json = StudentSerializer.Serialize(_student);
        var restored = StudentSerializer.Deserialize(json);
        Assert.Equal("Иван", restored.FirstName);
        Assert.Equal("Иванов", restored.LastName);
        Assert.Equal(new DateTime(2000, 5, 15), restored.BirthDate);
        Assert.Equal(2, restored.Grades.Count);
    }

    [Fact]
    public void SaveAndLoad_Works()
    {
        var path = Path.GetTempFileName();
        try
        {
            StudentSerializer.SaveToFile(_student, path);
            var loaded = StudentSerializer.LoadFromFile(path);
            Assert.Equal(_student.FirstName, loaded.FirstName);
        }
        finally
        {
            if (File.Exists(path)) File.Delete(path);
        }
    }

    [Fact]
    public void Serialize_IgnoresNull()
    {
        var student = new Student { FirstName = "Петр", LastName = null, Grades = null };
        var json = StudentSerializer.Serialize(student);
        Assert.DoesNotContain("lastName", json);
        Assert.DoesNotContain("grades", json);
    }

    [Fact]
    public void Deserialize_EmptyFirstName_Throws()
    {
        var json = StudentSerializer.Serialize(new Student { FirstName = "", LastName = "Иванов" });
        Assert.Throws<InvalidOperationException>(() => StudentSerializer.Deserialize(json));
    }

    [Fact]
    public void Deserialize_FutureBirthDate_Throws()
    {
        var student = new Student { FirstName = "Анна", LastName = "Петрова", BirthDate = DateTime.Now.AddDays(1) };
        var json = StudentSerializer.Serialize(student);
        Assert.Throws<InvalidOperationException>(() => StudentSerializer.Deserialize(json));
    }

    [Fact]
    public void Deserialize_Null_Throws()
    {
        Assert.Throws<InvalidOperationException>(() => StudentSerializer.Deserialize("null"));
    }
}
