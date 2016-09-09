﻿using Microsoft.AspNet.Identity.EntityFramework;
using System.Data.Entity;
using RazorEngineCms.Models;

namespace RazorEngineCms.DAL
{
    public class ApplicationContext : IdentityDbContext<ApplicationUser>
    {
        public ApplicationContext()
            : base("ApplicationContext")
        {
        }

        public DbSet<Page> Page { get; set; }

        public static ApplicationContext Create()
        {
            return new ApplicationContext();
        }
    }
}