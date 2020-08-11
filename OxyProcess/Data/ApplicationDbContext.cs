using System;
using System.Collections.Generic;
using System.Text;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using OxyProcess.Models;
using OxyProcess.Models.Account;
using OxyProcess.Models.FormTag;
using OxyProcess.Models.Group;
using OxyProcess.Models.Industry;
using OxyProcess.Models.SpecialPermission;
using OxyProcess.Models.Template;
using OxyProcess.Models.UserAddress;
using OxyProcess.Models.Worker;

namespace OxyProcess.Data
{
    public class ApplicationDbContext : IdentityDbContext<ApplicationUser, ApplicationRole, long>
    {
        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
            : base(options)
        {

        }
        public DbSet<Address> Addresses { get; set; }
       
        public DbSet<State> States { get; set; }
        public DbSet<Country> Countries { get; set; }
        public DbSet<Company> Companies { get; set; }
        public DbSet<Groups> Groups { get; set; }
        public DbSet<GroupMembers> GroupMembers { get; set; }
        public DbSet<GroupPermission> GroupPermission { get; set; }
        public DbSet<Template> Template { get; set; }
        public DbSet<TemplateType> TemplateType { get; set; }
        public DbSet<Tag> Tag { get; set; }
        public DbSet<industries> industries { get; set; }
        public DbSet<TaginsideTemplates> TaginsideTemplates { get; set; }
        public DbSet<TemplateFields> TemplateFields { get; set; }

        public DbSet<FilesManager> FilesManager { get; set; }

        public DbSet<TagFormDataEntry> TagFormDataEntry { get; set; }

        public DbSet<WorkerPasswords> WorkerPasswords { get; set; }
        public DbSet<SpecialPermission> SpecialPermission { get; set; }

        
        protected override void OnModelCreating(ModelBuilder builder)
        {
            base.OnModelCreating(builder);
            // Customize the ASP.NET Identity model and override the defaults if needed.
            // For example, you can rename the ASP.NET Identity table names and more.
            // Add your customizations after calling base.OnModelCreating(builder);


            ///<summary>
            /// UserMaster Changes
            ///</summary>
            builder.Entity<ApplicationUser>()
                .ToTable("Users_Master").Property(p => p.Id).HasColumnName("User_Id");
            builder.Entity<ApplicationUser>()
            .ToTable("Users_Master").Property(p => p.PhoneNumber).HasColumnName("Phone");
            builder.Entity<ApplicationUser>().ToTable("Users_Master").Ignore(p => p.TwoFactorEnabled);
            //builder.Entity<ApplicationUser>().ToTable("Users_Master").Ignore(p => p.NormalizedUserName);
            //builder.Entity<ApplicationUser>().ToTable("Users_Master").Ignore(p => p.NormalizedEmail);
            //builder.Entity<ApplicationUser>().ToTable("Users_Master").Ignore(p => p.SecurityStamp);
            builder.Entity<ApplicationUser>().ToTable("Users_Master").Ignore(p => p.LockoutEnd);
            builder.Entity<ApplicationUser>().ToTable("Users_Master").Ignore(p => p.LockoutEnabled);
            builder.Entity<ApplicationUser>().ToTable("Users_Master").Ignore(p => p.AccessFailedCount);
         
            ///<summary>
            /// User Roles
            ///</summary>
            builder.Entity<ApplicationRole>()
            .ToTable("Role").Property(p => p.Id).HasColumnName("Role_Id");


            List<string> states = new List<string>();
            states.Add("Alaska");
            states.Add("Arizona");
            states.Add("Arkansas");
            states.Add("California");
            states.Add("Colorado");
            states.Add("Connecticut");
            states.Add("Delaware");
            states.Add("Alabama");
            states.Add("District of Columbia");
            states.Add("Florida");
            states.Add("Georgia");
            states.Add("Hawaii");
            states.Add("Idaho");
            states.Add("Illinois");
            states.Add("Indiana");
            states.Add("Iowa");
            states.Add("Kansas");
            states.Add("Kentucky");
            states.Add("Louisiana");
            states.Add("Maine");
            states.Add("Maryland");
            states.Add("Massachusetts");
            states.Add("Michigan");
            states.Add("Minnesota");
            states.Add("Mississippi");
            states.Add("Missouri");
            states.Add("Montana");
            states.Add("Nebraska");
            states.Add("Nevada");
            states.Add("New Hampshire");
            states.Add("New Jersey");
            states.Add("New Mexico");
            states.Add("New York");
            states.Add("North Carolina");
            states.Add("North Dakota");
            states.Add("Ohio");
            states.Add("Oklahoma");
            states.Add("Oregon");
            states.Add("Pennsylvania");
            states.Add("Rhode Island");
            states.Add("South Carolina");
            states.Add("South Dakota");
            states.Add("Tennessee");
            states.Add("Texas");
            states.Add("Utah");
            states.Add("Vermont");
            states.Add("Virginia");
            states.Add("Washington");
            states.Add("West Virginia");
            states.Add("Wisconsin");
            states.Add("Wyoming");

            List<State> statelist = new List<State>();
            int i = 1;
           foreach(var s in states)
            {
                State st = new State();
                st.StateName = s;
                st.StateId = i;
                st.CountryId = 1;
                i = i + 1;
                statelist.Add(st);

            }
            


            //add Industry Type
            builder.Entity<industries>().HasData(
             new industries
             {
                 Id = 1,
                 industrieType = "Aerospace",
             },

             new industries
             {
                 Id = 2,
                 industrieType = "Automotive"
             },

             new industries
             {
                 Id = 3,
                 industrieType = "Bio - medical & Analytical"
             },

             new industries
             {
                 Id = 4,
                 industrieType = "Chemical Processing"
             },

             new industries
             {
                 Id = 5,
                 industrieType = "Defense"
             },

             new industries
             {
                 Id = 6,
                 industrieType = "Diesel & Gas Engines"
             },

             new industries
             {
                 Id = 7,
                 industrieType = "Distributor"
             },
             new industries
             {
                 Id = 8,
                 industrieType = "Food & Beverage"
             },

             new industries
             {
                 Id = 9,
                 industrieType = "Heavy Machinery / Equipment"
             },

             new industries
             {
                 Id = 10,
                 industrieType = "Industrial OEM"

             },
             new industries
             {
                 Id = 11,
                 industrieType = "Mechanical Seals"
             },
             new industries
             {
                 Id = 12,
                 industrieType = "Motorsport"
             },
             new industries
             {
                 Id = 13,
                 industrieType = "Oil & Gas"
             },
             new industries
             {
                 Id = 14,
                 industrieType = "Oil & Gas Pumps / Valves"
             },
             new industries
             {
                 Id = 15,
                 industrieType = "Petrochemical"
             },
             new industries
             {
                 Id = 16,
                 industrieType = "Pharmaceutical"

             },
             new industries
             {
                 Id = 17,
                 industrieType = "Power"
             },

             new industries
             {
                 Id = 18,
                 industrieType = "Pumps & Valves"
             },
             new industries
             {
                 Id = 19,
                 industrieType = "Other"
             }
          );




            //add Template Type

            builder.Entity<TemplateType>().HasData(
          new TemplateType
          {
              TypeId = 1,
              TemplateTypeName = "Regular Form"
          },
           new TemplateType
           {
               TypeId = 2,
               TemplateTypeName = "Report Card"
           }

           );

            //add dummy data for address dropdown
            builder.Entity<Country>().HasData(
         
           new Country
           {
               CountryId = 1,
               CountryName = "USA"
           }
);

            builder.Entity<State>().HasData(statelist);


            

        }
    }
}
