﻿using System.Numerics;

namespace SPG_Fachtheorie.Aufgabe1.Model
{
    public class Email 
    { 
        public string Value { get; set; }

        public Email(string value)
        {
            Value = value;
        }

        public static implicit operator Email(string value) => new Email(value);
        public static implicit operator string(Email email) => email.Value;
    }
    
}