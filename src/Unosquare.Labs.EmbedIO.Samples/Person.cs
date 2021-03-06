﻿#if !MONO
using System.ComponentModel.DataAnnotations.Schema;
using Unosquare.Labs.LiteLib;

namespace Unosquare.Labs.EmbedIO.Samples
{
    /// <summary>
    /// A simple model representing a person
    /// </summary>
    [Table("Persons")]
    public class Person : LiteModel
    {
        public string Name { get; set; }
        public int Age { get; set; }

        [LiteIndex]
        public string EmailAddress { get; set; }

        public string PhotoUrl => $"http://www.gravatar.com/avatar/{Extensions.ComputeMd5Hash(EmailAddress)}.png?s=100";
    }
}
#endif