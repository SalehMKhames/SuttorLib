using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using SuttorLibrary.Models;

namespace SuttorLibrary.Data
{
    public class AppDbContext(DbContextOptions options) 
        : IdentityDbContext<AppUser>(options)
    {
        public DbSet<AppUser> AppUsers { get; set; }
        public DbSet<Book> Books { get; set; }
        public DbSet<Author> Authors { get; set; }
        public DbSet<Category> Categories { get; set; }
        public DbSet<Download> Downloads { get; set; }
        public DbSet<BookRating> BookRatings { get; set; }
        public DbSet<BookAuthors> BookAuthors { get; set; }
        public DbSet<BookCategories> BookCategories { get; set; }
        public DbSet<UserInterests> UserInterests { get; set; }
        public DbSet<RefreshToken> RefreshTokens { get; set; }
        public DbSet<Languages> Languages { get; set; }
        public DbSet<BookLanguages> BookLanguages { get; set; }

    //protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
    //{
    //    optionsBuilder.ConfigureWarnings(w => w.Ignore(RelationalEventId.PendingModelChangesWarning));
    //}

    protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            modelBuilder.Entity<AppUser>()
                .HasIndex(x => x.UserName)
                .IsUnique();
                
            modelBuilder.Entity<AppUser>()
                .Property(u => u.IsAuthor)
                .HasDefaultValue(false);

            modelBuilder.Entity<Category>().HasKey(c => c.Id);
            modelBuilder.Entity<Category>()
                .HasIndex(c => c.Name)
                .IsUnique();

            modelBuilder.Entity<Book>().HasKey(b => b.Id);

            modelBuilder.Entity<Author>()
                .HasKey(a => a.Id);
            modelBuilder.Entity<Author>()
                .HasIndex(a => a.Name)
                .IsUnique();

            modelBuilder.Entity<BookRating>()
                .HasKey(br => br.Id);
            modelBuilder.Entity<BookRating>()
                .HasIndex(br => new { br.UserId, br.BookId });
            modelBuilder.Entity<BookRating>()
                .Property(br => br.Rating)
                .HasDefaultValue(0);

            modelBuilder.Entity<BookRating>()
                .HasOne<Book>()
                .WithMany()
                .HasForeignKey(br => br.BookId);
            modelBuilder.Entity<BookRating>()
                .HasOne<AppUser>()
                .WithMany()
                .HasForeignKey(br => br.UserId);

            modelBuilder.Entity<Download>()
                .HasKey(d => d.Id);
            modelBuilder.Entity<Download>()
                .HasOne<AppUser>()
                .WithMany()
                .HasForeignKey(d => d.UserID);
            modelBuilder.Entity<Download>()
                .HasOne<Book>()
                .WithMany()
                .HasForeignKey(d => d.BookID);
            modelBuilder.Entity<Download>()
                .Property(d => d.IsFinishReading)
                .HasDefaultValue(false);

            modelBuilder.Entity<UserInterests>()
                .HasKey(ui => ui.Id);
            modelBuilder.Entity<UserInterests>()
                .HasOne<AppUser>()
                .WithMany()
                .HasForeignKey(ui => ui.UserId);
            modelBuilder.Entity<UserInterests>()
                .HasOne<Category>()
                .WithMany()
                .HasForeignKey(ui => ui.Category_Id);

            modelBuilder.Entity<BookAuthors>()
                .HasKey(ba => ba.Id);
            modelBuilder.Entity<BookAuthors>()
                .HasOne<Book>()
                .WithMany()
                .HasForeignKey(ba => ba.Book_Id);
            modelBuilder.Entity<BookAuthors>()
                .HasOne<Author>()
                .WithMany()
                .HasForeignKey(ba => ba.Author_Id);

            modelBuilder.Entity<BookCategories>()
                .HasKey(bc => bc.Id);
            modelBuilder.Entity<BookCategories>()
                .HasOne<Category>()
                .WithMany()
                .HasForeignKey(bc => bc.categoryId);
            modelBuilder.Entity<BookCategories>()
                .HasOne<Book>()
                .WithMany()
                .HasForeignKey(bc => bc.bookId);

            modelBuilder.Entity<Languages>()
                .HasKey(t => t.Id);
            modelBuilder.Entity<Languages>()
                .HasIndex(t => t.LanguageCode)
                .IsUnique();
            modelBuilder.Entity<Languages>()
                .HasIndex(t => t.Language)
                .IsUnique();

            modelBuilder.Entity<BookLanguages>()
                .HasKey(bt => new { bt.LanguageId, bt.BookId});
            modelBuilder.Entity<BookLanguages>()
                .HasOne<Book>()
                .WithMany()
                .HasForeignKey(bt => bt.BookId);
            modelBuilder.Entity<BookLanguages>()
                .HasOne<Languages>()
                .WithMany()
                .HasForeignKey(bt => bt.LanguageId);

            // RefreshToken mapping
            modelBuilder.Entity<RefreshToken>()
                .HasKey(rt => rt.Id);
            modelBuilder.Entity<RefreshToken>()
                .HasIndex(rt => rt.Token)
                .IsUnique();
            modelBuilder.Entity<RefreshToken>()
                .HasOne<AppUser>()
                .WithMany()
                .HasForeignKey(rt => rt.UserId)
                .OnDelete(DeleteBehavior.Cascade);



            // -------- Identity seed data (Users + Roles) ---------

            // Stable GUIDs fro seeded data
            string adminRoleId = "d72cc571-c363-4d19-8818-9bebb24fba93";
            string userRoleId = "b28dafd1-5b59-47f2-8af2-d510cd1ecb0b";
            string authorRoleId = "60909110-f307-44d3-8d74-e9e85c5b7896";

            string adminId = "29d93d5b-efbc-4ac7-999b-7b211629d8b0";
            string userId = "f422f142-09b2-40b9-a886-a14b213973d5";
            string authorId = "b7359b68-b61a-4991-8c9f-b6394494b11a";

            var fixedJoinedAt = new DateTime(2024, 01, 01, 0, 0, 0, DateTimeKind.Utc);


            ////Roles first(Because the user entity has it as foreign key)
            modelBuilder.Entity<IdentityRole>()
                .HasData(
                    new IdentityRole
                    {
                        Id = adminRoleId,
                        Name = "Admin",
                        NormalizedName = "ADMIN",
                    },
                    new IdentityRole
                    {
                        Id = userRoleId,
                        Name = "User",
                        NormalizedName = "USER"
                    },
                    new IdentityRole
                    {
                        Id = authorRoleId,
                        Name = "Author",
                        NormalizedName = "AUTHOR"
                    }
                );

            AppUser admin = new AppUser
            {
                Id = adminId,
                UserName = "AdminSaleh",
                NormalizedUserName = "ADMINSALEH",
                FullName = "Saleh",
                Email = "salehalk512@gmail.com",
                NormalizedEmail = "SALEHALK512@GMAIL.COM",
                EmailConfirmed = true,
                JoinedAt = fixedJoinedAt,
                IsAuthor = false,
                XP = 100000,
                PhotoPath = "",
                IsAuthed = true,
                Token = "",
                ExpiresAt = DateTime.MinValue,
            };

            AppUser user = new AppUser
            {
                Id = userId,
                UserName = "User",
                NormalizedUserName = "USER",
                FullName = "Demo User",
                Email = "userDemo@example.com",
                NormalizedEmail = "USERDEMO@EXAMPLE.COM",
                EmailConfirmed = true,
                JoinedAt = fixedJoinedAt,
                IsAuthor = false,
                XP = 0,
                PhotoPath = "",
                IsAuthed = true,
                Token = "",
                ExpiresAt = DateTime.MinValue,
            };

            AppUser author = new AppUser
            {
                Id = authorId,
                UserName = "AuthorUser",
                NormalizedUserName = "Author",
                FullName = "Author User",
                Email = "authorDemo@example.com",
                NormalizedEmail = "AuthorDEMO@EXAMPLE.COM",
                EmailConfirmed = true,
                JoinedAt = fixedJoinedAt,
                IsAuthor = true,
                XP = 1000,
                PhotoPath = "",
                IsAuthed = true,
                Token = "",
                ExpiresAt = DateTime.MinValue,
            };


            PasswordHasher<AppUser> ph = new PasswordHasher<AppUser>();
            admin.PasswordHash = ph.HashPassword(admin, "Tbmfilj@72534");
            user.PasswordHash = ph.HashPassword(user, "UserDemo12345!");
            author.PasswordHash = ph.HashPassword(author, "AuthorDemo12345!");

            modelBuilder.Entity<AppUser>()
                .HasData(admin, user, author);


            ////Linking users to their roles
            modelBuilder.Entity<IdentityUserRole<string>>()
                .HasData(
                    new IdentityUserRole<string>
                    {
                        RoleId = adminRoleId,
                        UserId = adminId
                    },
                    new IdentityUserRole<string>
                    {
                        RoleId = userRoleId,
                        UserId = adminId
                    },
                    new IdentityUserRole<string>
                    {
                        RoleId = userRoleId,
                        UserId = userId
                    },
                    new IdentityUserRole<string>
                    {
                        RoleId = authorRoleId,
                        UserId = authorId
                    }
                );

            ////---------------Seeding Data in The Category Table----------------
            modelBuilder.Entity<Category>()
                .HasData(
                    new Category
                    {
                        Id = "948bccda-a227-4c74-8dc3-08c42ff714bb",
                        Name = "Science Fiction"
                    },
                    new Category
                    {
                        Id = "a27b68c5-c414-4f41-b31d-bf35eb668301",
                        Name = "Astronomy & Cosmology"
                    },
                    new Category
                    {
                        Id = "437dce48-242f-4d6c-af0c-b0c17ab5fff6",
                        Name = "Physics"
                    },
                    new Category
                    {
                        Id = "f0c835e7-3bc9-4c4b-84a9-62a4c38d7daa",
                        Name = "Biology"
                    },
                    new Category
                    {
                        Id = "5a881e12-9098-4e42-899a-1baf4e835419",
                        Name = "Neuroscience & Psychology"
                    },
                    new Category
                    {
                        Id = "5937f420-f649-4754-8c56-68328e0cac5d",
                        Name = "Chemistry"
                    },
                    new Category
                    {
                        Id = "23020a19-99a6-4930-9c3c-a2694e8d96d5",
                        Name = "Materials Science"
                    },
                    new Category
                    {
                        Id = "20e18efd-683c-4ff3-8dc7-841678f86b2d",
                        Name = "Mathematics"
                    },
                    new Category
                    {
                        Id = "bb916111-3739-47ca-9408-e314ced2a4bc",
                        Name = "Computer Science & A.I."
                    },
                    new Category
                    {
                        Id = "dc572a5c-5157-490a-8f74-100dfe6b13a9",
                        Name = "Ecology"
                    },
                    new Category
                    {
                        Id = "61982553-2d6a-4b67-95f2-97619dcc25ab",
                        Name = "Medicine & Public Health"
                    },
                    new Category
                    {
                        Id = "a317f95b-46d4-4f19-9279-dafa13958d87",
                        Name = "Geology"
                    },

                    new Category
                    {
                        Id = "1acf2f12-aef5-4fa9-8d73-383905631736",
                        Name = "Historical Fiction"
                    },
                    new Category
                    {
                        Id = "58a7a1eb-39b4-41fe-81f1-062550f5ded8",
                        Name = "Fantasy"
                    },
                    new Category
                    {
                        Id = "1b2aaa3f-2fdd-40e5-ac0a-c523962b4c21",
                        Name = "Mystery"
                    },
                    new Category
                    {
                        Id = "02c4f025-1871-46cf-80e0-ad120bd8f0aa",
                        Name = "Horror"
                    },
                    new Category
                    {
                        Id = "cdb07b90-6c8e-4233-9f68-169ce9a0c04b",
                        Name = "Romance"
                    },
                    new Category
                    {
                        Id = "bae8c4a4-a2ef-417e-b67c-603920a92038",
                        Name = "History"
                    },
                    new Category
                    {
                        Id = "834385e1-f0e8-4c4b-860d-e755ef5bc556",
                        Name = "Poetry"
                    },
                    new Category
                    {
                        Id = "2b59a98b-a317-44d8-8fe8-5451e24363ab",
                        Name = "Kids Stories"
                    },
                    new Category
                    {
                        Id = "a057aedb-7349-442b-a7d6-9a42e3b9fed6",
                        Name = "Magical Realism"
                    },
                    new Category
                    {
                        Id = "881f0e80-56ca-4793-8b56-6f0a415413a6",
                        Name = "Philosophy"
                    },
                    new Category
                    {
                        Id = "4471bb34-41a5-47b6-9391-d6cfa747b587",
                        Name = "Anthropology & Archaeology"
                    },
                    new Category
                    {
                        Id = "251991f8-e149-4235-81e1-951ede4de48b",
                        Name = "Sociology"
                    },
                    new Category
                    {
                        Id = "c61678ae-eaaa-452b-a538-28547502866b",
                        Name = "Political Science & Theory"
                    },
                    new Category
                    {
                        Id = "a759ecc3-aba2-48c5-990f-2fd411edf555",
                        Name = "Economics"
                    },
                    new Category
                    {
                        Id = "e9974cde-ece1-4da9-a1df-65df50a93a76",
                        Name = "Linguistics"
                    },
                    new Category
                    {
                        Id = "6ffc64fb-e474-4d8b-b995-ccdaeb94d99c",
                        Name = "Religious Studies & Theology"
                    },
                    new Category
                    {
                        Id = "bab76b06-5b80-4dd2-b38e-d393e8939fa4",
                        Name = "Art History & Criticism"
                    },
                    new Category
                    {
                        Id = "415fa2a8-64cb-4cb4-bb27-6aadfc51c760",
                        Name = "Music Theory & History"
                    },
                    new Category
                    {
                        Id = "3aca1245-631c-4736-95c0-54ac283db0ea",
                        Name = "Business & Entrepreneurship"
                    },
                    new Category
                    {
                        Id = "ff20e5cc-9c17-4f24-b003-33f68fd42175",
                        Name = "Personal Finance & Investing"
                    },
                    new Category
                    {
                        Id = "fbcba319-f818-48da-966b-1e4049a197b6",
                        Name = "Self-Improvement & Productivity"
                    },
                    new Category
                    {
                        Id = "b8e4cf35-867a-467c-817d-9500c0a14d8f",
                        Name = "Cooking & Culinary Arts"
                    },
                    new Category
                    {
                        Id = "04e9b182-8388-4faa-a00e-8d86b6f38427",
                        Name = "Travel & Adventure"
                    },
                    new Category
                    {
                        Id = "2dc6ab6e-c8b6-41e5-b9f6-45701946e466",
                        Name = "Sports & Athletics"
                    },
                    new Category
                    {
                        Id = "d4cd152a-2a92-4795-9ebb-95c9abc13fa7",
                        Name = "Gardening & Nature"
                    },
                    new Category
                    {
                        Id = "7e05aa6e-c409-429a-95ff-62dab8b76174",
                        Name = "Memoir & Autobiography"
                    },
                    new Category
                    {
                        Id = "6a75a2d4-3800-4498-b3e6-76fc6534d520",
                        Name = "Essays & Scientific Papers"
                    },
                    new Category
                    {
                        Id = "328b2074-3472-41ea-903e-a3558334a88a",
                        Name = "Comics & Graphic Novels"
                    },
                    new Category
                    {
                        Id = "92248135-1021-45cb-938f-76a976b8848e",
                        Name = "Photography"
                    },
                    new Category
                    {
                        Id = "d4bfc33e-5618-4dc6-900c-9fa16aed2534",
                        Name = "True Crime"
                    },
                    new Category
                    {
                        Id = "37e8584a-feb2-4061-be1d-fe1a94563c17",
                        Name = "Folklore & Mythology"
                    },
                    new Category
                    {
                        Id = "dd2880c3-7eee-4953-8e71-4e9016d95674",
                        Name = "Architecture & Design"
                    },
                    new Category
                    {
                        Id = "eeb2b037-8456-484a-ada9-1ce0c31d14f2",
                        Name = "Fashion & Style"
                    },
                    new Category
                    {
                        Id = "a97a7ee0-4133-4ca3-adc0-66fb582f2a8b",
                        Name = "Films & Television Studies"
                    },
                    new Category
                    {
                        Id = "32910cea-1cd4-4ebf-90cb-cb8621ee6161",
                        Name = "Technology Guides & Tutorials"
                    }
                );



            ////--------------Seeding Data in Language Table------------------

            modelBuilder.Entity<Languages>()
                .HasData(
                    new Languages
                    {
                        Id = "5b56f76e-a943-4a33-ae52-405f992c0ee1",
                        Language = "English",
                        LanguageCode = "en"
                    },
                    new Languages
                    {
                        Id = "1a88de30-80b1-4491-a6ec-333fa0174933",
                        Language = "Mandarin Chinese",
                        LanguageCode = "zh"
                    },
                    new Languages
                    {
                        Id = "a5bcf190-d4a5-4285-bfca-8e7888fede68",
                        Language = "Spanish",
                        LanguageCode = "es"
                    },
                    new Languages
                    {
                        Id = "163bd8eb-4d53-4a7b-a0bc-44d5b410c185",
                        Language = "Hindi",
                        LanguageCode = "hi"
                    },
                    new Languages
                    {
                        Id = "c7b70aeb-c5b3-437d-9f51-52a84634ad23",
                        Language = "Arabic",
                        LanguageCode = "ar"
                    },
                    new Languages
                    {
                        Id = "74ff7ceb-5df6-434c-a079-0650658df292",
                        Language = "Portuguese",
                        LanguageCode = "pt"
                    },
                    new Languages
                    {
                        Id = "c8a88ae4-18dc-44ed-b46e-9cb92f97a969",
                        Language = "Russian",
                        LanguageCode = "ru"
                    },
                    new Languages
                    {
                        Id = "593e8d92-6b37-4fff-a00c-41197b4b6386",
                        Language = "Japanese",
                        LanguageCode = "ja"
                    },
                    new Languages
                    {
                        Id = "7580183c-ea56-490c-9880-e665b73b634e",
                        Language = "French",
                        LanguageCode = "fr"
                    },
                    new Languages
                    {
                        Id = "c367d2d7-1850-4e99-af79-26f433383b8c",
                        Language = "German",
                        LanguageCode = "de"
                    },
                    new Languages
                    {
                        Id = "0eb15275-2db6-4cd4-b6f0-915d2fe49503",
                        Language = "Korean",
                        LanguageCode = "ko"
                    },
                    new Languages
                    {
                        Id = "4f4d5e66-ca69-4364-a6c6-7ac7e6748248",
                        Language = "Italian",
                        LanguageCode = "it"
                    },
                    new Languages
                    {
                        Id = "db567ea5-c771-43da-a1d8-e0d67e411177",
                        Language = "Turkish",
                        LanguageCode = "tr"
                    },
                    new Languages
                    {
                        Id = "a7381524-0b42-4105-b41e-d7576bb7faf4",
                        Language = "Polish",
                        LanguageCode = "pl"
                    },
                    new Languages
                    {
                        Id = "87f53ee0-6654-49dd-93ec-49f553819714",
                        Language = "Dutch",
                        LanguageCode = "nl"
                    },
                    new Languages
                    {
                        Id = "41fe6770-e040-416e-adf4-85075230e3ba",
                        Language = "Swedish",
                        LanguageCode = "sv"
                    },
                    new Languages
                    {
                        Id = "9fbe1970-694f-4613-b7a4-a3ce4e50a3f4",
                        Language = "Greek",
                        LanguageCode = "el"
                    },
                    new Languages
                    {
                        Id = "a291c167-e5bb-42dc-a4f5-617e4f1ec055",
                        Language = "Czech",
                        LanguageCode = "cs"
                    },
                    new Languages
                    {
                        Id = "20c60f9d-d8ad-498c-a960-02ccdec83a63",
                        Language = "Romanian",
                        LanguageCode = "ro"
                    },
                    new Languages
                    {
                        Id = "8f3b108f-1b75-4b5a-8b2e-be5c91ef8f80",
                        Language = "Hungarian",
                        LanguageCode = "hu"
                    },
                    new Languages
                    {
                        Id = "e3661f50-9ba2-45d6-b2c7-f367637e00dc",
                        Language = "Thai",
                        LanguageCode = "th"
                    },
                    new Languages
                    {
                        Id = "4dda4135-d8e2-4bd1-aaea-c66bb06f0489",
                        Language = "Vietnamese",
                        LanguageCode = "vi"
                    },
                    new Languages
                    {
                        Id = "35923bc4-c519-4748-a17a-8fc6f030dde2",
                        Language = "Filipino",
                        LanguageCode = "fil"
                    },
                    new Languages
                    {
                        Id = "63769d61-fd77-4b02-9ab5-bb3d8de5dd09",
                        Language = "Indonesian",
                        LanguageCode = "id"
                    },
                    new Languages
                    {
                        Id = "ca83826e-5a1d-444b-a95e-7e03853933f0",
                        Language = "Malay",
                        LanguageCode = "ms"
                    },
                    new Languages
                    {
                        Id = "4b452292-7c43-4b83-9b99-8821f84a3e40",
                        Language = "Bengali",
                        LanguageCode = "bn"
                    },
                    new Languages
                    {
                        Id = "b41fd658-2252-423f-a891-4176875dd4c5",
                        Language = "Urdu",
                        LanguageCode = "ur"
                    },
                    new Languages
                    {
                        Id = "490e8980-67c0-4cc5-bad0-9cd022bbef49",
                        Language = "Hebrew",
                        LanguageCode = "he"
                    },
                    new Languages
                    {
                        Id = "c54fc06d-e0e7-4686-91f9-1b4885fb85d7",
                        Language = "Persian",
                        LanguageCode = "fa"
                    },
                    new Languages
                    {
                        Id = "33165761-6725-4d75-af4b-6794a72e9270",
                        Language = "Danish",
                        LanguageCode = "da"
                    },
                    new Languages
                    {
                        Id = "3691f261-a07b-40dd-bcf6-c48a91d31347",
                        Language = "Norwegian",
                        LanguageCode = "no"
                    },
                    new Languages
                    {
                        Id = "471ec16b-4a25-49e6-a1a4-9e9fb24d06d0",
                        Language = "Finnish",
                        LanguageCode = "fi"
                    },
                    new Languages
                    {
                        Id = "f982b583-a50c-491e-9ddc-e434542fc643",
                        Language = "Ukrainian",
                        LanguageCode = "uk"
                    },
                    new Languages
                    {
                        Id = "f83d2700-7f50-4a49-8885-e53dce76c117",
                        Language = "Serbian",
                        LanguageCode = "sr"
                    },
                    new Languages
                    {
                        Id = "4ef36e82-cc2b-4e52-8fa1-8b1c46e86c0b",
                        Language = "Bulgarian",
                        LanguageCode = "bg"
                    },
                    new Languages
                    {
                        Id = "6a3e435d-0fb4-4698-b83d-6e9dc0e9c3e0",
                        Language = "Croatian",
                        LanguageCode = "hr"
                    },
                    new Languages
                    {
                        Id = "ed35ec1c-5556-4553-b8f1-d2f0794a4066",
                        Language = "Slovenian",
                        LanguageCode = "sl"
                    },
                    new Languages
                    {
                        Id = "2f811e46-26d8-4606-9e22-7abbce66f697",
                        Language = "Slovak",
                        LanguageCode = "sk"
                    },
                    new Languages
                    {
                        Id = "da42ed01-8bd6-4d4b-a501-c406b8b64df8",
                        Language = "Lithuanian",
                        LanguageCode = "lt"
                    },
                    new Languages
                    {
                        Id = "d0e3574e-3a64-420b-85bf-88c5ff1562fd",
                        Language = "Estonian",
                        LanguageCode = "et"
                    },
                    new Languages
                    {
                        Id = "7c44e07d-f447-48c6-9b99-7dff1596f5cc",
                        Language = "Latvian",
                        LanguageCode = "lv"
                    },
                    new Languages
                    {
                        Id = "5cd5b25e-7685-4181-9c6a-c22761acc729",
                        Language = "Irish",
                        LanguageCode = "ga"
                    },
                    new Languages
                    {
                        Id = "2df2ecaf-fe98-46f5-8663-a724b63a1ab8",
                        Language = "Punjabi",
                        LanguageCode = "pa"
                    }
                );


            ////---------------Seeding Data in Authors Table------------------
            modelBuilder.Entity<Author>()
                .HasData(
                    new Author
                    {
                        Id = "e9761dbd-fe17-4902-a55d-5719d6895146",
                        Name = "Fyodor Dostoevsky",
                        Description = "Fyodor Mikhailovich Dostoevsky (11 November [O.S. 30 October] 1821 – 9 February [O.S. 28 January] 1881) " +
                        "was a Russian philosopher, novelist, short story writer, essayist and journalist." +
                        " He is regarded as one of the greatest novelists in both Russian and world literature, and many of his works are considered highly influential masterpieces." +
                        "Dostoevsky's literary works explore the human condition in the troubled political, social and spiritual atmospheres of 19th-century Russia, " +
                        "and engage with a variety of philosophical and religious themes. His most acclaimed novels include Crime and Punishment (1866), The Idiot (1869), Demons (1872), " +
                        "The Adolescent (1875) and The Brothers Karamazov (1880). His Notes from Underground, a novella published in 1864, " +
                        "is considered one of the first works of existentialist literature."
                    },
                    new Author
                    {
                        Id = "8e8dcbf0-2e52-42f6-8ee8-ffa962a6b8e9",
                        Name = "Anton Chekhov",
                        Description = "Anton Pavlovich Chekhov[a] ( Russian: Антон Павлович Чехов,29 January 1860 – 15 July 1904)" +
                        " was a Russian playwright and short-story writer, widely considered to be one of the greatest writers of all time. " +
                        "His career as a playwright produced four classics, and his best short stories are held in high esteem by writers and critics. " +
                        "Along with Henrik Ibsen and August Strindberg, Chekhov is often referred to as one of the three seminal figures in the birth of early modernism in the theatre." +
                        " Chekhov was a physician by profession. \"Medicine is my lawful wife,\" he once said, \"and literature is my mistress.\"\n" +
                        "Chekhov renounced the theatre after the reception of The Seagull in 1896, but the play was revived to acclaim in 1898 by Konstantin Stanislavski's Moscow Art Theatre, " +
                        "which subsequently also produced Chekhov's Uncle Vanya and premiered his last two plays, Three Sisters and The Cherry Orchard. " +
                        "These four works present a challenge to the acting ensemble as well as to audiences, because in place of conventional action Chekhov offers a \"theatre of mood\" and a \"submerged life in the text.\"" +
                        " The plays that Chekhov wrote were not complex, and created a somewhat haunting atmosphere for the audience." +
                        "\r\nChekhov began writing stories to earn money, but as his artistic ambition grew, he made formal innovations that influenced the evolution " +
                        "of the modern short story. He made no apologies for the difficulties this posed to readers, insisting that the role of an artist was to ask questions," +
                        " not to answer them."
                    },
                    new Author
                    {
                        Id = "efbc8877-e432-4ede-8b0b-6cdb92e97476",
                        Name = "Leo Tolstoy",
                        Description = "Count Lev Nikolayevich Tolstoy (Russian: Лев Николаевич Толстой,; 9 September [O.S. 28 August] 1828 – 20 November [O.S. 7 November] 1910), " +
                        "usually referred to in English as Leo Tolstoy, was a Russian writer. He is regarded as one of the greatest and most influential authors of all time." +
                        "\r\n\r\nBorn to an aristocratic family, Tolstoy achieved acclaim in his twenties with his semi-autobiographical trilogy, Childhood, Boyhood and Youth (1852–1856), " +
                        "and with Sevastopol Sketches (1855), based on his experiences in the Crimean War. His War and Peace (1869), Anna Karenina (1878), and Resurrection (1899), " +
                        "which is based on his \"youthful sins,\" are often cited as pinnacles of realist fiction and three of the greatest novels ever written. " +
                        "His oeuvre includes short stories such as \"Alyosha the Pot\" (1911) and \"After the Ball\" (1911) and novellas such as Family Happiness (1859), " +
                        "The Death of Ivan Ilyich (1886), The Kreutzer Sonata (1889), The Devil (1911), and Hadji Murat (1912). He also wrote plays and essays concerning philosophical, " +
                        "moral and religious themes.\r\n\r\nIn the 1870s, Tolstoy experienced a profound moral crisis, followed by what he regarded as an equally profound spiritual awakening, " +
                        "as outlined in his non-fiction work Confession (1882). His literal interpretation of the ethical teachings of Jesus, centering on the Sermon on the Mount, " +
                        "caused him to become a fervent Christian anarchist and pacifist. His ideas on nonviolent resistance, expressed in such works as The Kingdom of God Is Within You (1894)," +
                        " had a profound impact on such pivotal 20th-century figures as Mahatma Gandhi, Ludwig Wittgenstein, Martin Luther King Jr., and James Bevel." +
                        " He also became a dedicated advocate of Georgism, the economic philosophy of Henry George, which he incorporated into his writing, " +
                        "particularly in his novel Resurrection (1899).\r\n\r\nTolstoy received praise from countless authors and critics, both during his lifetime and after." +
                        " Virginia Woolf called Tolstoy \"the greatest of all novelists\",[10] and Gary Saul Morson referred to War and Peace as the greatest of all novels." +
                        " He received nominations for the Nobel Prize in Literature every year from 1902 to 1906 and for the Nobel Peace Prize in 1901, 1902, and 1909. " +
                        "Tolstoy never being awarded a Nobel Prize remains a major Nobel Prize controversy."
                    },
                    new Author
                    {
                        Id = "ed99c064-7b5f-4135-b0a2-27788f18bbd5",
                        Name = "Franz Kafka",
                        Description = "Franz Kafka (3 July 1883 – 3 June 1924) was a German-language Czech writer and novelist born in Prague, in the Austro-Hungarian Empire. " +
                        "Widely regarded as a major figure of 20th-century literature, his works fuse elements of realism and the fantastique, and typically feature isolated protagonists" +
                        " facing bizarre or surreal predicaments and incomprehensible socio-bureaucratic powers. The term Kafkaesque has entered the lexicon to describe situations " +
                        "like those depicted in his writings. His best-known works include the novella The Metamorphosis (1915) and the novels The Trial (1924) and The Castle (1926). " +
                        "He is also celebrated for his brief fables and aphorisms, which frequently incorporated comedic elements alongside the darker themes of his longer works." +
                        " His work has widely influenced artists, philosophers, composers, filmmakers, literary historians, religious scholars, and cultural theorists, " +
                        "and his writings have been seen as prophetic or premonitory of a totalitarian future.\r\n\r\nKafka was born into a middle-class German- and Yiddish-speaking Czech" +
                        " family in Prague, the capital of the Kingdom of Bohemia, which belonged to the Austro-Hungarian Empire (later the capital of Czechoslovakia and the Czech Republic)." +
                        " He trained as a lawyer, and after completing his legal education was employed full-time in various legal and insurance jobs. " +
                        "His professional obligations led to internal conflict as he felt that his true vocation was writing. Only a minority of his works were published during his life; " +
                        "the story collections Contemplation (1912) and A Country Doctor (1919), and individual stories, such as his novella The Metamorphosis, were published in literary " +
                        "magazines, but they received little attention. He wrote hundreds of letters to family and close friends, including his father, with whom he had a strained and formal" +
                        " relationship. He became engaged to several women but never married. He died relatively unknown in 1924 of tuberculosis, aged 40. " +
                        "His literary executor and friend Max Brod ignored Kafka's wishes to destroy his remaining works, publishing them to eventual acclaim."
                    },
                    new Author
                    {
                        Id = "8baa507b-fe2b-42f5-9296-b7aedfbfc596",
                        Name = "Virginia Woolf",
                        Description = "Adeline Virginia Woolf (25 January 1882 – 28 March 1941) was an English writer and one of the most influential 20th-century modernist authors. " +
                        "She helped to pioneer the use of stream of consciousness narration as a literary device.\r\n\r\nVirginia Woolf was born in South Kensington, London, into an affluent " +
                        "and intellectual family as the seventh child of Julia Prinsep Jackson and Leslie Stephen. She grew up in a blended household of eight children, including her sister, " +
                        "the painter Vanessa Bell. Educated at home in English classics and Victorian literature, Woolf later attended King’s College London, where she studied classics and " +
                        "history and encountered early advocates for women’s rights and education.\r\n\r\nAfter the death of her father in 1904, Woolf and her family moved to the bohemian " +
                        "Bloomsbury district, where she became a founding member of the influential Bloomsbury Group. She married Leonard Woolf in 1912, and together they established " +
                        "the Hogarth Press in 1917, which published much of her work. They eventually settled in Sussex in 1940, maintaining their involvement in literary circles throughout " +
                        "their lives.\r\n\r\nWoolf began publishing professionally in 1900 and rose to prominence during the interwar period with novels like Mrs Dalloway (1925), " +
                        "To the Lighthouse (1927), and Orlando (1928), as well as the feminist essay A Room of One’s Own (1929). Her work became central to 1970s feminist criticism and remains " +
                        "influential worldwide, having been translated into over 50 languages. Woolf’s legacy endures extensive scholarship, cultural portrayals, and tributes such as memorials, " +
                        "societies, and university buildings bearing her name."
                    },
                    new Author
                    {
                        Id = "7be65b0d-3c1d-49d3-a529-e1bb1055331a",
                        Name = "Gabriel García Márquez",
                        Description = "Gabriel José García Márquez (Latin American Spanish: 6 March 1927 – 17 April 2014) was a Colombian writer and journalist, known affectionately as Gabo " +
                        "or Gabito throughout Latin America. Considered one of the most significant authors of the 20th century, particularly in the Spanish language, " +
                        "he was awarded the 1972 Neustadt International Prize for Literature and the 1982 Nobel Prize in Literature. He pursued a self-directed education that resulted in leaving " +
                        "law school for a career in journalism. From early on he showed no inhibitions in his criticism of Colombian and foreign politics. In 1958, " +
                        "he married Mercedes Barcha Pardo; they had two sons, Rodrigo and Gonzalo.\r\n\r\nGarcía Márquez started as a journalist and wrote many acclaimed non-fiction works and " +
                        "short stories. He is best known for his novels, such as No One Writes to the Colonel (1961), One Hundred Years of Solitude (1967), which has sold over fifty million " +
                        "copies worldwide, Chronicle of a Death Foretold (1981), and Love in the Time of Cholera (1985). His works have achieved significant critical acclaim and widespread " +
                        "commercial success, most notably for popularizing a literary style known as magic realism, which uses magical elements and events in otherwise ordinary and realistic " +
                        "situations. Some of his works are set in the fictional village of Macondo (mainly inspired by his birthplace, Aracataca), and most of them explore the theme of solitude. " +
                        "He is the most-translated Spanish-language author. In 1982, he was awarded the Nobel Prize in Literature, \"for his novels and short stories, in which the fantastic and " +
                        "the realistic are combined in a richly composed world of imagination, reflecting a continent's life and conflicts\". He was the fourth Latin American to receive the honor," +
                        " following Chilean poets Gabriela Mistral (1945) and Pablo Neruda (1971), as well as Guatemalan novelist Miguel Ángel Asturias (1967). Alongside Jorge Luis Borges, " +
                        "García Márquez is regarded as one of the most renowned Latin American authors in history.\r\n\r\nUpon García Márquez's death in April 2014, Juan Manuel Santos, " +
                        "the president of Colombia, called him \"the greatest Colombian who ever lived.\""
                    },
                    new Author
                    {
                        Id = "d7a1117e-5f37-4859-81a1-9bf677d5a876",
                        Name = "Stephen Hawking",
                        Description = "Stephen William Hawking (8 January 1942 – 14 March 2018) was an English theoretical astrophysicist, cosmologist, and author who was director of " +
                        "research at the Centre for Theoretical Cosmology at the University of Cambridge. Between 1979 and 2009, he was the Lucasian Professor of Mathematics at Cambridge, " +
                        "widely viewed as one of the most prestigious academic posts in the world.\r\n\r\nHawking was born in Oxford into a family of physicians. In October 1959, " +
                        "at the age of 17, he began his university education at University College, Oxford, where he received a first-class BA degree in physics. In October 1962, he began " +
                        "his graduate work at Trinity Hall, Cambridge, where, in March 1966, he obtained his PhD in applied mathematics and theoretical physics, specialising in general " +
                        "relativity and cosmology. In 1963, at age 21, Hawking was diagnosed with an early-onset slow-progressing form of motor neurone disease that gradually, " +
                        "over decades, paralysed him. After the loss of his speech, he communicated through a speech-generating device, initially through use of a handheld switch, " +
                        "and eventually by using a single cheek muscle.\r\n\r\nHawking's scientific works included a collaboration with Roger Penrose on gravitational singularity theorems in the " +
                        "framework of general relativity, and the theoretical prediction that black holes emit radiation, often called Hawking radiation. Initially, Hawking radiation was controversial." +
                        " By the late 1970s, and following the publication of further research, the discovery was widely accepted as a major breakthrough in theoretical physics. " +
                        "Hawking was the first to set out a theory of cosmology explained by a union of the general theory of relativity and quantum mechanics. Hawking was a vigorous supporter of the" +
                        " many-worlds interpretation of quantum mechanics. He also introduced the notion of a micro black hole.\r\n\r\nHawking achieved commercial success with several works of popular" +
                        " science in which he discussed his theories and cosmology in general. His book A Brief History of Time appeared on the Sunday Times bestseller list for a record-breaking 237 " +
                        "weeks. Hawking was a Fellow of the Royal Society, a lifetime member of the Pontifical Academy of Sciences, and a recipient of the Presidential Medal of Freedom, " +
                        "the highest civilian award in the United States. In 2002, Hawking was ranked number 25 in the BBC's poll of the 100 Greatest Britons. He died in 2018 at the age of 76, " +
                        "having lived more than 50 years following his diagnosis of motor neurone disease."
                    },
                    new Author
                    {
                        Id = "2049063d-9270-43a6-9288-a80627e0be80",
                        Name = "George R. R. Martin",
                        Description = "George Raymond Richard Martin (born George Raymond Martin; September 20, 1948), also known by the initials G.R.R.M., is an American author, " +
                        "television writer, and television producer. Martin is best known as the author of the epic fantasy novel series A Song of Ice and Fire, which have been adapted " +
                        "by HBO into the Primetime Emmy Award–winning television series Game of Thrones (2011–2019) and its prequel series House of the Dragon (2022–present); " +
                        "Martin also wrote a related series of novellas, Tales of Dunk and Egg, which are being adapted by HBO as A Knight of the Seven Kingdoms (2026–present). " +
                        "Outside of A Song of Ice and Fire and its related media, Martin helped create the Wild Cards anthology series and contributed worldbuilding for the " +
                        "video game Elden Ring (2022).\r\n\r\nIn 2005, Lev Grossman of Time called Martin \"the American Tolkien\", and in 2011, he was included on the annual Time 100 " +
                        "list of the most influential people in the world. He is a longtime resident of Santa Fe, New Mexico, where he helped fund Meow Wolf and owns the Jean Cocteau " +
                        "Cinema. The city commemorates March 29 as George R. R. Martin Day."
                    },
                    new Author
                    {
                        Id = "edfe02ee-24ae-4b94-a99d-c7bf42df3292",
                        Name = "Neil deGrasse Tyson",
                        Description = "Neil deGrasse Tyson (born October 5, 1958) is an American astrophysicist, author, and science communicator. Tyson studied at Harvard University, " +
                        "the University of Texas at Austin, and Columbia University. From 1991 to 1994, he was a postdoctoral research associate at Princeton University. In 1994, " +
                        "he joined the Hayden Planetarium as a staff scientist and the Princeton faculty as a visiting research scientist and lecturer. In 1996, he became director of the " +
                        "planetarium and oversaw its $210 million reconstruction project, which was completed in 2000. Since 1996, he has been the director of the Hayden Planetarium at " +
                        "the Rose Center for Earth and Space in New York City. The center is part of the American Museum of Natural History, where Tyson founded the Department of Astrophysics " +
                        "in 1997 and has been a research associate in the department since 2003.\r\n\r\nFrom 1995 to 2005, Tyson wrote monthly essays in the \"Universe\" column for Natural " +
                        "History magazine, some of which were later published in his books Death by Black Hole (2007) and Astrophysics for People in a Hurry (2017). During the same period, " +
                        "he wrote a monthly column in StarDate magazine, answering questions about the universe under the pen name \"Merlin\". Material from the column appeared in his books " +
                        "Merlin's Tour of the Universe (1998) and Just Visiting This Planet (1998). Tyson served on a 2001 government commission on the future of the U.S. aerospace industry " +
                        "and on the 2004 Moon, Mars and Beyond commission. He was awarded the NASA Distinguished Public Service Medal in the same year. From 2006 to 2011, he hosted the " +
                        "television show NOVA ScienceNow on PBS. Since 2009, Tyson has hosted the weekly podcast StarTalk. A spin-off, also called StarTalk, began airing on National Geographic in 2015." +
                        " In 2014, he hosted the television series Cosmos: A Spacetime Odyssey, a successor to Carl Sagan's 1980 series Cosmos: A Personal Voyage. The U.S. National Academy of Sciences " +
                        "awarded Tyson the Public Welfare Medal in 2015 for his \"extraordinary role in exciting the public about the wonders of science\"."
                    },
                    new Author
                    {
                        Id = "4d8243ec-f8f6-47df-b5f7-fa99b2627784",
                        Name = "Carl Sagan",
                        Description = "Carl Edward Sagan (November 9, 1934 – December 20, 1996) was an American astronomer, planetary scientist and science communicator. Initially an " +
                        "assistant professor at Harvard, Sagan later moved to Cornell, where he was the David Duncan Professor of Astronomy and Space Sciences and directed the Laboratory " +
                        "for Planetary Studies. He published more than 600 scientific papers and articles and several popular science books, starting with The Cosmic Connection." +
                        " He won the Pulitzer Prize for General Nonfiction for The Dragons of Eden.\r\n\r\nHe co-wrote and narrated the 1980 documentary series Cosmos: A Personal Voyage, " +
                        "which has been seen by at least 500 million people in 60 countries and won two Emmy Awards and a Peabody Award. Cosmos, the companion volume, was the bestselling " +
                        "science book to date. A lifelong science-fiction fan, Sagan turned his pen to the genre with Contact, which was adapted as the film of the same name." +
                        "\r\n\r\nHe had a lifelong interest in the possibility of extraterrestrial life and contributed to the Arecibo message, the Pioneer plaques and the Voyager Golden " +
                        "Record, universal messages that could potentially be understood by any intelligence that might find them. He promoted skepticism and the scientific method, " +
                        "particularly in his penultimate book The Demon-Haunted World. In it, he popularized a toolkit for critical thinking. He made famous the maxim \"Extraordinary " +
                        "claims require extraordinary evidence.\" He proposed the Pale Blue Dot photograph of Earth taken by Voyager 1. The phrase \"Billions and billions\" was attributed " +
                        "to him, although he never said it. He did use it as the title of his last book. Sagan received numerous awards and honors, including the NASA Distinguished Public " +
                        "Service Medal and the National Academy of Sciences Public Welfare Medal. He married three times and had five children. After developing myelodysplasia, " +
                        "Sagan died of pneumonia at the age of 62 on December 20, 1996."
                    },
                    new Author
                    {
                        Id = "e74ce8f5-c6aa-4827-96dd-0193cd373e62",
                        Name = "Sean Carroll",
                        Description = "Sean Michael Carroll (born October 5, 1966) is an American theoretical physicist who specializes in quantum mechanics, cosmology, " +
                        "and the philosophy of science. He is the Homewood Professor of Natural Philosophy at Johns Hopkins University. He was formerly a research professor at " +
                        "the Walter Burke Institute for Theoretical Physics at the California Institute of Technology (Caltech) department of physics. He also is currently an external " +
                        "professor at the Santa Fe Institute, and he has been a contributor to the physics blog Cosmic Variance, where he has published in scientific journals such as " +
                        "Nature as well as other publications, including The New York Times, Sky & Telescope, and New Scientist. He is known for his atheism, his vocal critique of " +
                        "theism and defense of naturalism. He is considered a prolific public speaker and science popularizer. In 2007, Carroll was named NSF Distinguished Lecturer by " +
                        "the National Science Foundation.\r\n\r\nHe has appeared on the History Channel's The Universe, Science Channel's Through the Wormhole with Morgan Freeman, " +
                        "Closer to Truth (broadcast on PBS), and Comedy Central's The Colbert Report. Carroll is the author of Spacetime And Geometry, a graduate-level textbook in general" +
                        " relativity, and has also recorded lectures for The Great Courses on cosmology, time in physics and the Higgs boson. He is also the author of four popular books: " +
                        "From Eternity to Here about the arrow of time, The Particle at the End of the Universe about the Higgs boson, The Big Picture: On the Origins of Life, Meaning, " +
                        "and the Universe Itself about ontology, and Something Deeply Hidden about the foundations of quantum mechanics.\r\n\r\nIn 2018, Carroll began a podcast called" +
                        " Mindscape, in which he interviews other experts and intellectuals coming from a variety of disciplines, including \"science, society, philosophy, culture, arts " +
                        "and ideas\" in general. He has also published a YouTube video series entitled \"The Biggest Ideas in the Universe\" which provides physics instruction at a " +
                        "popular-science level but with equations and a mathematical basis, rather than mere analogy. The series has become the basis of a new book series with the " +
                        "installment, The Biggest Ideas in the Universe: Space, Time, and Motion, published in September 2022 and the second volume, Quanta and Fields, " +
                        "published in May 2024, with the third and final volume pending publication"
                    },
                    new Author
                    {
                        Id = "ee012952-686e-4dd0-a58c-26f45540a2ca",
                        Name = "Charles Darwin",
                        Description = "Charles Robert Darwin (12 February 1809 – 19 April 1882) was an English naturalist, geologist, and biologist, widely known for his contributions to evolutionary biology." +
                        " His proposition that all species of life have descended from a common ancestor is now generally accepted and considered a fundamental scientific concept. " +
                        "In a joint presentation with Alfred Russel Wallace, he introduced his scientific theory that this branching pattern of evolution resulted from a process he called natural selection, " +
                        "in which the struggle for existence has a similar effect to the artificial selection involved in selective breeding. Darwin has been described as one of the most influential figures in" +
                        " human history and was honoured by burial in Westminster Abbey.\r\n\r\nDarwin's early interest in nature led him to neglect his medical education at the University of Edinburgh; " +
                        "instead, he helped Robert Edmond Grant to investigate marine invertebrates. His studies at the University of Cambridge's Christ's College from 1828 to 1831 encouraged his passion " +
                        "for natural science. However, it was his five-year voyage on HMS Beagle from 1831 to 1836 that truly established Darwin as an eminent geologist. The observations and theories he " +
                        "developed during his voyage supported Charles Lyell's concept of gradual geological change. Publication of his journal of the voyage made Darwin famous as a popular author. " +
                        "His first scientific work was The Structure and Distribution of Coral Reefs (1842). Along with his work on barnacles, it won him the Royal Medal in 1853." +
                        "\r\n\r\nPuzzled by the geographical distribution of wildlife and fossils he collected on the voyage, Darwin began detailed investigations and, in 1838, devised his theory of natural" +
                        " selection. Although he discussed his ideas with several naturalists, he needed time for extensive research, and his geological work had priority. He was writing up his theory in 1858" +
                        " when Wallace sent him an essay that described the same idea, prompting the immediate joint submission of both their theories to the Linnean Society of London. Darwin's work established" +
                        " evolutionary descent with modification as the dominant scientific explanation of natural diversification. Darwin published his theory of evolution with compelling evidence in On the " +
                        "Origin of Species (1859). He explored coevolution in Fertilisation of Orchids (1862) and human evolution and sexual selection in The Descent of Man, and Selection in Relation to Sex " +
                        "(1871). The Expression of the Emotions in Man and Animals (1872) was an early work of psychology, and one of the first books to feature photographs. His final book was The Formation of " +
                        "Vegetable Mould, through the Actions of Worms (1881).\r\n\r\nBy the 1870s, the scientific community and a majority of the educated public had accepted evolution as a fact. " +
                        "However, many initially favoured competing explanations that gave only a minor role to natural selection. It was not until the emergence of the modern evolutionary synthesis from the " +
                        "1930s to the 1950s that a broad consensus developed in which natural selection was the basic mechanism of evolution. Darwin's discovery is the unifying theory of the life sciences, " +
                        "explaining the unity and diversity of life."
                    },
                    new Author
                    {
                        Id = "3e8d1068-6a4f-4786-b48b-e110ee1061a4",
                        Name = "Richard Dawkins",
                        Description = "Richard Dawkins (born 26 March 1941) is a British evolutionary biologist, zoologist, science communicator and author. He is an emeritus fellow of New College, Oxford. " +
                        "In 1995 he was named the first Simonyi Professor for the Public Understanding of Science, a position he held until 2008, and is on the advisory board of the University of Austin." +
                        " Dawkins has won several academic and writing awards, among them the 2005 Shakespeare Prize and the 2006 Lewis Thomas Prize. Presenting the latter, Paul Nurse said \"In eloquent, " +
                        "evocative prose, Richard Dawkins conveys the certainty that, rather than diminishing the myriad beauties of the universe and extinguishing wonderment at its mysteries, " +
                        "science reveals truths that are yet more awe-inspiring than the mysteries they solve.\"\r\n\r\nDawkins has written a number of popular books explicating evolution. " +
                        "In The Selfish Gene (1976), he popularised the gene-centred view of evolution and coined the word meme. In The Blind Watchmaker (1986), he explains how the cumulative, " +
                        "non-random process of natural selection, coupled with random variation, creates complexity. In Climbing Mount Improbable (1996), he explores how evolution gradually creates " +
                        "complex adaptations through a series of intermediates. The book grew out of his Royal Institution Christmas Lectures, Growing Up in the Universe. With Yan Wong, " +
                        "he co-authored The Ancestor's Tale (2004), a \"Chaucerian pilgrimage to the dawn of life\".\r\n\r\nAlong with Christopher Hitchens, Daniel Dennett and Sam Harris, he is known as one" +
                        " of the \"Four Horsemen of the New Atheism.\" He made the case for atheism in The God Delusion (2006). The Sunday Times described it as one of the 12 most influential books since " +
                        "the Second World War. That year he founded the Richard Dawkins Foundation for Reason and Science. He edited The Oxford Book of Modern Science Writing (2008) and authored a children's" +
                        " book, The Magic of Reality (2011). He has published two volumes of memoirs, An Appetite for Wonder (2013) and Brief Candle in the Dark (2015)."
                    },
                    new Author
                    {
                        Id = "3ee308c6-3016-4cc8-ab89-a48a60e518a4",
                        Name = "E.O. Wilson",
                        Description = "Edward Osborne Wilson ForMemRS (June 10, 1929 – December 26, 2021) was an American biologist, naturalist, ecologist, and entomologist known for " +
                        "developing the field of sociobiology.\r\n\r\nBorn in Alabama, Wilson found an early interest in nature and frequented the outdoors. At age seven, he was partially " +
                        "blinded in a fishing accident; due to his reduced sight, Wilson resolved to study entomology. After graduating from the University of Alabama, he earned his " +
                        "doctorate at Harvard University, where he distinguished himself in multiple fields. In 1956, he co-authored a paper defining the theory of character displacement." +
                        " In 1967, he developed the theory of island biogeography with Robert MacArthur.\r\n\r\nWilson was the Pellegrino University Research Professor Emeritus in " +
                        "Entomology for the Department of Organismic and Evolutionary Biology at Harvard University, a lecturer at Duke University, and a fellow of the Committee for " +
                        "Skeptical Inquiry. The Royal Swedish Academy awarded Wilson the Crafoord Prize. He was a humanist laureate of the International Academy of Humanism. He was a " +
                        "two-time winner of the Pulitzer Prize for General Nonfiction (for On Human Nature in 1979, and The Ants in 1991) and a New York Times bestselling author for The " +
                        "Social Conquest of Earth, Letters to a Young Scientist, and The Meaning of Human Existence.\r\n\r\nWilson's work received both praise and criticism during his " +
                        "lifetime. His 1975 book Sociobiology: The New Synthesis was a particular flashpoint for controversy, and drew criticism from the Sociobiology Study Group. " +
                        "Wilson's interpretation of the theory of evolution resulted in a widely reported dispute with Richard Dawkins about multilevel selection theory. Examinations of " +
                        "his letters after his death revealed that he had supported the psychologist J. Philippe Rushton, whose work on race and intelligence is widely regarded by the " +
                        "scientific community as deeply flawed and racist."
                    },
                    new Author
                    {
                        Id = "0c238a31-fa21-469f-903a-b0b075244eac",
                        Name = "Albert Einstein",
                        Description = "Albert Einstein (14 March 1879 – 18 April 1955) was a German-born theoretical physicist best known for developing the theory of relativity. " +
                        "Einstein also made important contributions to quantum theory. His mass–energy equivalence formula E = mc2, which arises from special relativity, " +
                        "has been called \"the world's most famous equation\". He received the 1921 Nobel Prize in Physics for \"his services to theoretical physics, and especially for his " +
                        "discovery of the law of the photoelectric effect\".\r\n\r\nBorn in the German Empire, Einstein moved to Switzerland in 1895, forsaking his German citizenship (as a subject of the " +
                        "Kingdom of Württemberg) the following year. In 1897, at the age of seventeen, he enrolled in the mathematics and physics teaching diploma program at the Swiss federal polytechnic " +
                        "school in Zurich, graduating in 1900. He acquired Swiss citizenship a year later, which he kept for the rest of his life, and afterwards secured a permanent position at the Swiss " +
                        "Patent Office in Bern. In 1905, he submitted a successful PhD dissertation to the University of Zurich. In 1914, he moved to Berlin to join the Prussian Academy of Sciences and the" +
                        " Humboldt University of Berlin, becoming director of the Kaiser Wilhelm Institute for Physics in 1917; he also became a German citizen again, this time as a subject of the Kingdom " +
                        "of Prussia. In 1933, while Einstein was visiting the United States, Adolf Hitler came to power in Germany. Horrified by the Nazi persecution of his fellow Jews, he decided " +
                        "to remain in the US, and was granted American citizenship in 1940. On the eve of World War II, he endorsed a letter to President Franklin D. Roosevelt alerting him to the potential" +
                        " German nuclear weapons program and recommending that the US begin similar research, later carried out as the Manhattan Project.\r\n\r\nIn 1905, sometimes described as his annus " +
                        "mirabilis (miracle year), he published four groundbreaking papers. In them, he outlined a theory of the photoelectric effect, explained Brownian motion, " +
                        "introduced his special theory of relativity, and demonstrated that if the special theory is correct, mass and energy are equivalent to each other. In 1915, he " +
                        "proposed a general theory of relativity that extended his system of mechanics to incorporate gravitation. A paper that he published the following year laid out the" +
                        " implications of general relativity for the modeling of the structure and evolution of the universe as a whole. It introduced the cosmological constant and" +
                        " is further regarded as the first step in the field of modern theoretical cosmology. In 1917, Einstein wrote a paper which introduced the concepts of spontaneous " +
                        "emission and stimulated emission, the latter of which is the core mechanism behind the laser and maser, and which contained a trove of information that would be " +
                        "beneficial to developments in physics later on, such as quantum electrodynamics and quantum optics.\r\n\r\nIn the middle part of his career, Einstein made important " +
                        "contributions to statistical mechanics and quantum theory. Especially notable was his work on the quantum physics of radiation, in which light consists of particles, " +
                        "subsequently called photons. With physicist Satyendra Nath Bose, he laid the groundwork for Bose–Einstein statistics. For much of the last phase of his academic life," +
                        " Einstein worked on two endeavors that ultimately proved unsuccessful. First, he advocated against quantum theory's introduction of fundamental randomness into" +
                        " science's picture of the world, objecting that \"God does not play dice\". Second, he attempted to devise a unified field theory by generalizing his geometric" +
                        " theory of gravitation to include electromagnetism. As a result, he became increasingly isolated from mainstream modern physics. Many things are named after him, " +
                        "including the element Einsteinium. In 1999, he was named Time's Person of the Century."
                    },
                    new Author
                    {
                        Id = "d3f2725b-2fd7-481b-948a-bf24b3362eab",
                        Name = "Oliver Sacks",
                        Description = "Oliver Wolf Sacks (9 July 1933 – 30 August 2015) was a British neurologist, naturalist, historian of science, and writer.\r\n\r\nBorn in London, " +
                        "Sacks received his medical degree in 1958 from The Queen's College, Oxford, before moving to the United States, where he spent most of his career. He interned at" +
                        " Mount Zion Hospital in San Francisco and completed his residency in neurology and neuropathology at the University of California, Los Angeles (UCLA). Later, " +
                        "he served as neurologist at Beth Abraham Hospital's chronic-care facility in the Bronx, where he worked with a group of survivors of the 1920s sleeping sickness" +
                        " encephalitis lethargica epidemic, who had been unable to move on their own for decades. His treatment of those patients became the basis of his 1973 book " +
                        "Awakenings, which was adapted into an Academy Award-nominated feature film, in 1990, starring Robin Williams and Robert De Niro. His other best-selling " +
                        "books were mostly collections of case studies of people, including himself, with neurological disorders. He also published hundreds of articles " +
                        "(both peer-reviewed scientific articles and articles for a general audience), about neurological disorders, history of science, natural history, and nature." +
                        " Journals and letters written by Sacks, but discovered after his death, indicate that some of his work was embellished or exaggerated.\r\n\r\nThe New York Times" +
                        " called him a \"poet laureate of contemporary medicine\", and \"one of the great clinical writers of the 20th century\". Some of his books were adapted for plays" +
                        " by major playwrights, feature films, animated short films, opera, dance, fine art, and musical works in the classical genre. His book The Man Who Mistook " +
                        "His Wife for a Hat, which describes the case histories of some of his patients, became the basis of an opera of the same name. The television series Brilliant" +
                        " Minds is based on his life."
                    },
                    new Author
                    {
                        Id = "e166ab33-e284-4324-95ea-fb3b9348fbde",
                        Name = "Neils Bohr",
                        Description = "Niels Henrik David Bohr (7 October 1885 – 18 November 1962) was a Danish theoretical physicist who made foundational contributions to understanding" +
                        " atomic structure and quantum theory, for which he received the Nobel Prize in Physics in 1922. He was also a philosopher and a promoter of scientific research." +
                        "\r\n\r\nBohr developed the Bohr model of the atom, in which he proposed that energy levels of electrons are discrete and that the electrons revolve in stable orbits" +
                        " around the atomic nucleus but can jump from one energy level (or orbit) to another. Although the Bohr model has been supplanted by other models, its underlying" +
                        " principles remain valid. He conceived the principle of complementarity: that items could be separately analysed in terms of contradictory properties, " +
                        "like behaving as a wave or a stream of particles. The notion of complementarity dominated Bohr's thinking in both science and philosophy.\r\n\r\nBohr " +
                        "founded the Institute of Theoretical Physics at the University of Copenhagen, now known as the Niels Bohr Institute, which opened in 1920. Bohr mentored" +
                        " and collaborated with physicists including Hans Kramers, Oskar Klein, George de Hevesy, and Werner Heisenberg. He predicted the properties of a new " +
                        "zirconium-like element, which was named hafnium, after the Latin name for Copenhagen, where it was discovered. Later, the synthetic element bohrium was named" +
                        " after him because of his groundbreaking work on the structure of atoms.\r\n\r\nDuring the 1930s, Bohr helped refugees from Nazism. After Denmark was occupied" +
                        " by the Germans, he met with Heisenberg, who had become the head of the German nuclear weapon project. In September 1943 word reached Bohr that he was about to" +
                        " be arrested by the Germans, so he fled to Sweden. From there, he was flown to Britain, where he joined the British Tube Alloys nuclear weapons project, and was" +
                        " part of the British mission to the Manhattan Project. After the war, Bohr called for international cooperation on nuclear energy. He was involved with the" +
                        " establishment of CERN and the Research Establishment Risø of the Danish Atomic Energy Commission and became the first chairman of the Nordic Institute for " +
                        "Theoretical Physics in 1957. In 1999, he was named the fourth greatest physicist of all time."
                    },
                    new Author
                    {
                        Id = "d2d4665f-2cc0-4181-84e1-8ae896c33496",
                        Name = "Antonio Damasio",
                        Description = "Antonio Damasio (born 25 February 1944) is a Portuguese neuroscientist. He is currently the David Dornsife Chair in Neuroscience, as well as " +
                        "Professor of Psychology, Philosophy, and Neurology, at the University of Southern California, and, additionally, an adjunct professor at the Salk Institute." +
                        " He was previously the chair of neurology at the University of Iowa for 20 years. Damasio heads the Brain and Creativity Institute, and has authored several books." +
                        " Damasio's research in neuroscience has shown that emotions play a central role in social cognition and decision-making."
                    },
                    new Author
                    {
                        Id = "7c154694-cfa4-43ea-868e-cd7ecdb43641",
                        Name = "Lisa Feldman Barrett",
                        Description = "Lisa Feldman Barrett is a Canadian-American psychologist. She is a Distinguished Professor of psychology at Northeastern University, where she " +
                        "focuses on affective science and co-directs the Interdisciplinary Affective Science Laboratory. She has received the William James Fellow Award from the " +
                        "Association for Psychological Science for 2025, and the Award for Distinguished Scientific Contributions from the American Psychological Association for 2021, " +
                        "as well as a Guggenheim Fellowship. Along with James Russell, she is the founding editor-in-chief of the journal Emotion Review. Along with James Gross, " +
                        "she founded the Society for Affective Science."
                    },
                    new Author
                    {
                        Id = "a0882ae2-3942-4ef6-958b-896b367eba96",
                        Name = "V.S. Ramachandran",
                        Description = "Vilayanur Subramanian Ramachandran (born 10 August 1951) is an Indian-American neuroscientist. He is known for his experiments and theories in " +
                        "behavioral neurology, including the invention of the mirror box. Ramachandran is a distinguished professor in UCSD's Department of Psychology, where he is the " +
                        "director of the Center for Brain and Cognition.\r\n\r\nAfter earning a medical degree in India, Ramachandran studied experimental neuroscience at Cambridge, " +
                        "obtaining his PhD there in 1978. Most of his research has been in the fields of behavioral neurology and visual psychophysics. After early work on human " +
                        "vision, Ramachandran turned to work on wider aspects of neurology including phantom limbs and phantom pain. Ramachandran also performed the world's first " +
                        "\"phantom limb amputation\" surgeries by inventing the mirror therapy, which is now widely used for reducing phantom pains (with the goal of eliminating phantom" +
                        " sensations altogether in long term), and also for helping to restore motor control in stroke victims with weakened limbs.\r\n\r\nRamachandran's books Phantoms in" +
                        " the Brain (1998), The Tell-Tale Brain (2010), and others describe neurological and clinical studies of people with synesthesia, Capgras syndrome, and a wide range" +
                        " of other unusual conditions. Ramachandran has also described his work in many public lectures, including lectures for the BBC, and two official TED talks."
                    },
                    new Author
                    {
                        Id = "eb886e63-b3ce-4fa9-8199-cb3b482ca035",
                        Name = "Marie Curie",
                        Description = "Maria Salomea Skłodowska Curie(7 November 1867 – 4 July 1934), better known as Marie Curie , was a Polish and naturalised-French physicist and " +
                        "chemist. She shared the 1903 Nobel Prize in Physics with her husband Pierre Curie \"for their joint researches on the radioactivity phenomena discovered by " +
                        "Professor Henri Becquerel\". She won the 1911 Nobel Prize in Chemistry \"[for] the discovery of the elements radium and polonium, by the isolation of radium and" +
                        " the study of the nature and compounds of this remarkable element\".\r\n\r\nShe was the first woman to win a Nobel Prize, the first person to win a Nobel Prize" +
                        " twice, and the only person to win a Nobel Prize in two different scientific fields. Marie and Pierre were first married couple to win the Nobel Prize and" +
                        " launching the Curie family legacy of five Nobel Prizes. She was, in 1906, the first woman to become a professor at the University of Paris.\r\n\r\nShe was born" +
                        " in Warsaw, in what was then the Kingdom of Poland, part of the Russian Empire. She studied at Warsaw's clandestine Flying University and began her practical" +
                        " scientific training in Warsaw. In 1891, aged 24, she followed her elder sister Bronisława to study in Paris, where she earned her higher degrees and conducted" +
                        " her subsequent scientific work. In 1895, she married Pierre Curie, with whom she conducted pioneering research on radioactivity—a term she coined. In 1906, " +
                        "Pierre died in a Paris street accident.\r\n\r\nUnder her direction, the world's first studies were conducted into the treatment of neoplasms by the use " +
                        "of radioactive isotopes. She founded the Curie Institute in Paris in 1920, and the Curie Institute in Warsaw in 1932; both remain major medical research " +
                        "centres. During World War I, she developed mobile radiography units to provide X-ray services to field hospitals.\r\n\r\nWhile a French citizen, Marie Skłodowska" +
                        " Curie, who used both surnames, never lost her sense of Polish identity. She taught her daughters the Polish language and took them on visits to Poland. " +
                        "She named the first chemical element she and Pierre discovered polonium, after her native country.\r\n\r\nMarie Curie died in 1934, aged 66, at the Sancellemoz" +
                        " sanatorium in Passy (Haute-Savoie), France, of aplastic anaemia likely from exposure to radiation in the course of her scientific research and in the course of " +
                        "her radiological work at field hospitals during World War I. In addition to her Nobel Prizes, she received numerous other honours and tributes; in 1995 she " +
                        "became the first woman to be entombed on her own merits in the Paris Panthéon, and Poland declared 2011 the Year of Marie Curie during the International Year" +
                        " of Chemistry. She is the subject of numerous biographies, including Madame Curie by her daughter Ève"
                    },
                    new Author
                    {
                        Id = "3ec6a6f3-8048-4642-903e-6c2bf8333ca0",
                        Name = "Ibn Khaldun",
                        Description = "Ibn Khaldun (27 May 1332 – 17 March 1406, 732–808 AH) was an Arab scholar, historian, philosopher, and sociologist. He is widely acknowledged to be" +
                        " one of the greatest social scientists of the Middle Ages, and considered by a number of scholars to be a major forerunner of historiography, sociology, economics" +
                        ", and demography studies.\r\n\r\nHis best-known book is the Muqaddimah or Prolegomena (\"Introduction\"), which he wrote in six months as he states in his " +
                        "autobiography. It later influenced 17th-century and 19th-century Ottoman historians such as Kâtip Çelebi, Mustafa Naima and Ahmed Cevdet Pasha, who used its " +
                        "theories to analyze the growth and decline of the Ottoman Empire. Ibn Khaldun interacted with Tamerlane, the founder of the Timurid Empire.\r\n\r\nHe has been " +
                        "called one of the most prominent Muslim and Arab scholars and historians. Recently, Ibn Khaldun's works have been compared with those of influential European " +
                        "philosophers such as Niccolò Machiavelli, Giambattista Vico, David Hume, G. W. F. Hegel, Karl Marx, and Auguste Comte as well as the economists David Ricardo " +
                        "and Adam Smith, suggesting that their ideas found precedent (although not direct influence) in his. He has also been influential on certain modern Islamic thinkers" +
                        " (e.g. those of the traditionalist school)."
                    },
                    new Author
                    {
                        Id = "e69a8300-c804-4f12-b3e7-6465e31cbcd9",
                        Name = "Ibn Sina",
                        Description = "Ibn Sina (c. 980 – 22 June 1037), commonly known in the West as Avicenna (/ˌævɪˈsɛnə, ˌɑːv-/ A(H)V-ih-SEN-ə), was a preeminent philosopher " +
                        "and physician of the Muslim world. He was a seminal figure of the Islamic Golden Age, serving in the courts of various Iranian rulers, and was influential to" +
                        " medieval European medical and Scholastic thought.\r\n\r\nOften described as the father of early modern medicine, Avicenna's most famous works are The Book of" +
                        " Healing, a philosophical and scientific encyclopedia, and The Canon of Medicine, a medical encyclopedia that became a standard medical text at many medieval" +
                        " European universities and remained in use as late as 1650.\r\n\r\nBesides philosophy and medicine, Avicenna's corpus includes writings on astronomy, alchemy," +
                        " geography and geology, psychology, Islamic theology, logic, mathematics, physics, and works of poetry. His philosophy was of the Peripatetic school derived" +
                        " from Aristotelianism, of which he is considered among the greatest proponents within the Muslim world.\r\n\r\nAvicenna wrote most of his philosophical and " +
                        "scientific works in Arabic but also wrote several key works in Persian; his poetry was written in both languages. Of the 450 works he is believed to have written," +
                        " around 240 have survived, including 150 on philosophy and 40 on medicine."
                    },
                    new Author
                    {
                        Id = "fb082272-063a-4389-8dca-4e83e19cafb6",
                        Name = "Ibn Rushd",
                        Description = "Ibn Rushd (14 April 1126 – 11 December 1198), Latinized as Averroes, was an Andalusian polymath and jurist who was proficient in a variety of " +
                        "intellectual fields, including philosophy, theology, medicine, astronomy, physics, psychology, mathematics, neurology, Islamic jurisprudence and law, and " +
                        "linguistics. The author of more than 100 books and treatises, his philosophical works include numerous commentaries on Aristotle, for which he was known in " +
                        "the Western world as \"The Commentator\" and \"Father of Rationalism\".\r\n\r\nAverroes was a strong proponent of Aristotelianism; he attempted to restore what" +
                        " he considered the original teachings of Aristotle and opposed the Neoplatonist tendencies of earlier Muslim thinkers, such as al-Farabi and Avicenna." +
                        " He also defended the pursuit of philosophy against criticism by Ash'ari theologians such as Al-Ghazali. Averroes argued that philosophy was permissible in Islam" +
                        " and even compulsory among certain elites. He also argued scriptural text should be interpreted allegorically if it appeared to contradict conclusions reached by" +
                        " reason and philosophy. In Islamic jurisprudence, he wrote the Bidāyat al-Mujtahid on the differences between Islamic schools of law and the principles" +
                        " that caused their differences. In medicine, he proposed a new theory of stroke, described the signs and symptoms of Parkinson's disease for the first time," +
                        " and might have been the first to identify the retina as the part of the eye responsible for sensing light. His medical book Al-Kulliyat fi al-Tibb, translated" +
                        " into Latin and known as the Colliget, became a textbook in Europe for centuries.\r\n\r\nHis legacy in the Islamic world was modest for geographical and " +
                        "intellectual reasons. In the West, Averroes was known for his extensive commentaries on Aristotle, many of which were translated into Latin and Hebrew. The" +
                        " translations of his work reignited Western European interest in Aristotle and Greek thinkers, an area of study that had been widely abandoned after the fall" +
                        " of the Western Roman Empire. His thoughts generated controversies in Latin Christendom and triggered a philosophical movement called Averroism based on his" +
                        " writings. His unity of the intellect thesis, proposing that all humans share the same intellect, became one of the best-known and most controversial Averroist" +
                        " doctrines in the West. His works were condemned by the Catholic Church in 1270 and 1277. Although weakened by condemnations and sustained critique from Thomas " +
                        "Aquinas, Latin Averroism continued to attract followers up to the sixteenth century."
                    },
                    new Author
                    {
                        Id = "9039f00a-f4e3-4282-a374-cb38edf9b09d",
                        Name = "Al-Khwarizmi",
                        Description = "Muhammad ibn Musa al-Khwarizmi, or simply al-Khwarizmi (c. 780 – c. 850) was a mathematician active during the Islamic Golden Age, who produced " +
                        "Arabic-language works in mathematics, astronomy, and geography. Around 820, he worked at the House of Wisdom in Baghdad, the contemporary capital city of the" +
                        " Abbasid Caliphate. One of the most prominent scholars of the period, his works were widely influential on later authors, both in the Islamic world and Europe." +
                        "\r\n\r\nHis popularizing treatise on algebra, compiled between 813 and 833 as Al-Jabr (The Compendious Book on Calculation by Completion and Balancing),   " +
                        "presented the first systematic solution of linear and quadratic equations. One of his achievements in algebra was his demonstration of how to solve quadratic " +
                        "equations by completing the square, for which he provided geometric justifications.  Because al-Khwarizmi was the first person to treat algebra as an independent" +
                        " discipline and introduced the methods of \"reduction\" and \"balancing\" (the transposition of subtracted terms to the other side of an equation, that is, the " +
                        "cancellation of like terms on opposite sides of the equation), he has been described as the father or founder of algebra. The English term algebra comes from" +
                        " the short-hand title of his aforementioned treatise (الجبر Al-Jabr, transl. \"completion\" or \"rejoining\"). His name gave rise to the English terms algorism" +
                        " and algorithm; the Spanish, Italian, and Portuguese terms algoritmo; and the Spanish term guarismo and Portuguese term algarismo, all meaning 'digit'." +
                        "\r\n\r\nIn the 12th century, Latin translations of al-Khwarizmi's textbook on Indian arithmetic (Algorithmo de Numero Indorum), which codified the various Indian" +
                        " numerals, introduced the decimal-based positional number system to the Western world. Likewise, Al-Jabr, translated into Latin by the English scholar Robert of" +
                        " Chester in 1145, was used until the 16th century as the principal mathematical textbook of European universities.\r\n\r\nAl-Khwarizmi revised Geography, " +
                        "the 2nd-century Greek-language treatise by Ptolemy, listing the longitudes and latitudes of cities and localities.  He further produced a set of astronomical " +
                        "tables and wrote about calendric works, as well as the astrolabe and the sundial. Al-Khwarizmi made important contributions to trigonometry, producing accurate" +
                        " sine and cosine tables."
                    },
                    new Author
                    {
                        Id = "e63670dc-f437-4891-becf-c031424e4378",
                        Name = "Ibn Battuta",
                        Description = "Ibn Battuta (/24 February 1304 – 1368/1369) was a Maghrebi Muslim traveller, explorer and scholar. Over a period of 30 years from 1325 to 1354, " +
                        "he visited much of Africa, Asia, and the Iberian Peninsula. Near the end of his life, Ibn Battuta dictated an account of his journeys, titled A Gift to Those" +
                        " Who Contemplate the Wonders of Cities and the Marvels of Travelling, commonly known as The Rihla. Ibn Battuta travelled more than any other explorer in pre-" +
                        "modern history, totalling around 117,000 km (73,000 mi), surpassing Zheng He with about 50,000 km (31,000 mi) and Marco Polo with 24,000 km (15,000 mi)"
                    },
                    new Author
                    {
                        Id = "25ecc614-5e01-4228-9025-02a4b65125c8",
                        Name = "Ibn Al-Haytham",
                        Description = "Ibn al-Haytham, Latinized as Alhazen (c. 965 – c. 1040) was a mathematician, astronomer, and physicist of the Islamic Golden Age from present-day Iraq." +
                        "Referred to as \"the father of modern optics\", he made significant contributions to the principles of optics and visual perception in particular. His most " +
                        "influential work is titled Kitāb al-Manāẓir (Arabic: كتاب المناظر, \"Book of Optics\"), written during 1011–1021, which survived in a Latin edition. The works " +
                        "of Alhazen were frequently cited during the Scientific Revolution by Galileo Galilei, René Descartes, Johannes Kepler, and Christiaan Huygens.\r\n\r\nIbn " +
                        "al-Haytham was the first to correctly explain vision as intromissive rather than extramissive, and to argue that vision occurs in the brain, pointing to " +
                        "observations that it is subjective and affected by personal experience. He also stated the principle of least time for refraction which would later become " +
                        "Fermat's principle. He made major contributions to catoptrics and dioptrics by studying reflection, refraction and nature of images formed by light rays. " +
                        "Ibn al-Haytham was an early proponent of the concept that a hypothesis must be supported by experiments based on confirmable procedures or mathematical reasoning" +
                        " – an early pioneer in the scientific method five centuries before Renaissance scientists, he is sometimes described as the world's \"first true scientist\". " +
                        "He was also a polymath, writing on philosophy, theology and medicine.\r\n\r\nBorn in Basra, he spent most of his productive period in the Fatimid capital of " +
                        "Cairo and earned his living authoring various treatises and tutoring members of the nobilities. Ibn al-Haytham is sometimes given the byname al-Baṣrī after his" +
                        " birthplace, or al-Miṣrī (\"the Egyptian\"). Al-Haytham was dubbed the \"Second Ptolemy\" by Abu'l-Hasan Bayhaqi and \"The Physicist\" by John Peckham. Ibn " +
                        "al-Haytham paved the way for the modern science of physical optics."
                    },
                    new Author
                    {
                        Id = "aaf67e2b-61de-455f-94d9-cfd0bb0f2e88",
                        Name = "Ibn Taymiyya",
                        Description = "Ibn Taymiyya (Arabic: ٱبْن تَيْمِيَّة; 22 January 1263 – 26 September 1328) was a Sunni Muslim scholar, jurist, Mujtahid, traditionist, Qadiri Sufi," +
                        " proto-Salafi theologian and iconoclast. Born in Harran in 1263 CE and fleeing from the Mongol invasion, he was taught by his grandfather and father in the" +
                        " principles of Islamic Jurisprudence at Damascus. Ibn Taymiyya proved to be a controversial figure among both his contemporaries and in later centuries." +
                        " Clerics and state authorities accused Ibn Taymiyya and his disciples of anthropomorphism, which eventually led to the censoring of his works and subsequent" +
                        " incarceration.\r\n\r\nNevertheless, he emerged as one of the forefathers of the Salafi movement. Throughout his treatises, Ibn Taymiyya asserted there is no" +
                        " contradiction between reason and revelation, and denounced the usage of philosophy as a pre-requisite in seeking religious truth. As a cleric who viewed " +
                        "Shiism as a source of corruption in Muslim societies, Ibn Taymiyya was also known for his anti-Shia polemics throughout treatises such as Minhaj al-Sunna, " +
                        "wherein he denounced the Imami Shia creed as heretical. He issued a ruling to wage jihad against the Shias of Kisrawan and personally fought in the Kisrawan" +
                        " campaigns himself, accusing Shias of acting as the fifth-columnists of the Frank Crusaders and Mongol Ilkhanids. He is also known for his diplomatic involvement" +
                        " with the Ilkhanid ruler Ghazan Khan at the Battle of Marj al-Saffar, which ended the Mongol invasions of the Levant. A legal jurist of the Hanbali school, " +
                        "Ibn Taymiyya's condemnation of numerous Sufi practices associated with saint veneration and visitation of tombs made him a controversial figure with many rulers" +
                        " and scholars of the time, which caused him to be imprisoned several times as a result.\r\n\r\nWithin recent history, Ibn Taymiyya has been widely regarded as a" +
                        " major scholarly influence in militant Islamist movements, such as Salafi jihadism. Major aspects of his teachings, had a profound influence on Muhammad ibn " +
                        "Abd al-Wahhab, the founder of the Wahhabism reform movement formed in the Arabian Peninsula, as well as other later Sunni scholars. Syrian Salafi theologian" +
                        " Muhammad Rashid Rida, one of the major modern proponents of Ibn Taymiyya's works, designated him as the Mujaddid of the 7th Islamic century. Ibn Taymiyya's" +
                        " doctrinal positions, such as his takfir of the Mongol Ilkhanids and allowing jihad against other Muslims, were referenced by later Islamist political " +
                        "movements, including the Muslim Brotherhood, Hizb ut-Tahrir, al-Qaeda, and Islamic State, to justify social uprisings against the contemporary governments" +
                        " of the Muslim world. Ibn Taymiyya has been used by Wahhabism and Salafism for anti-Sufi rhetorics. However, from a historical perspective, this is an" +
                        " inaccurate portrayal of ibn Taimiyya, based on selective and out-of-context use of some of his writings by fundamentalist movements. While he sometimes criticized" +
                        " certain practices associated with Sufism, he acknowledged that Sufism is an integral part of Islam and praised many Sufi masters; he himself was affiliated" +
                        " with the Qadiriyya order. He also heavily condemned the killing of civilians, calling it \"corruption that Allah and His Messenger hates.\""
                    },
                    new Author
                    {
                        Id = "671fcbf5-46e5-421b-8fbc-ef3d86502e23",
                        Name = "Ibn Qayyim al-Jawziyya",
                        Description = "Shams ad-Dīn ʾabū ʿAbd Allāh Muḥammad ibn ʾAbī Bakr ibn ʾAyyūb az-Zurʿī ad-Dimashqī al-Ḥanbalī (29 Jan. 1292–15 Sep. 1350 CE / 691–751 AH), " +
                        "commonly known as Ibn Qayyim al-Jawziyyah (\"The son of the principal of [the school of] Jawziyyah\") or Ibn al-Qayyim (\"Son of the principal\"; ابن القيّم) " +
                        "for short, or reverentially as al-Imam Ibn al-Qayyim in Sunni tradition, was an important medieval Islamic jurisconsult, theologian, and spiritual writer. " +
                        "Belonging to the Hanbali school of Fiqh (Islamic Jurisprudence), of which he is regarded as \"one of the most important thinkers,\" Ibn al-Qayyim was also" +
                        " the foremost disciple and student of Ibn Taymiyyah, with whom he was imprisoned in 1326 for dissenting against established tradition during Ibn Taymiyyah's" +
                        " famous incarceration in the Citadel of Damascus.\r\n\r\nOf humble origin, Ibn al-Qayyim's father was the principal (qayyim) of the School of Jawziyyah, " +
                        "which also served as a court of law for the Hanbali judge of Damascus during the period. Ibn al-Qayyim went on to become a prolific scholar, producing a rich" +
                        " corpus of \"doctrinal and literary\" works. As a result, numerous important Muslim scholars of the Mamluk period were among Ibn al-Qayyim's students or, at" +
                        " least, greatly influenced by him, including, amongst others, the Shafi historian Ibn Kathir (d. 774/1373), the Hanbali hadith scholar Ibn Rajab (d. 795/1397)" +
                        " and Ibn Hajar al-Asqalani (d. 852/1449). In the present day, Ibn al-Qayyim's name has become a controversial one in certain quarters of the Islamic world due to" +
                        " his popularity amongst many adherents of Salafism, who see in his criticisms of such widespread Sufi practices of the medieval period associated with veneration" +
                        " of saints and the veneration of their graves and relics a classical precursor to their own perspective."
                    },
                    new Author
                    {
                        Id = "185ed5ca-0ac3-4dee-8054-14e20608b9d9",
                        Name = "Ibn Al-Kathir",
                        Description = "Abu al-Fida Isma'il ibn Umar ibn Kathir al-Dimashqi (Arabic: أبو الفداء إسماعيل بن عمر بن كثير الدمشقي, romanized: Abū al-Fidā' Ismā'īl ibn 'Umar " +
                        "ibn Kathīr al-Dimashqī; c. 1300–1373), known simply as Ibn Kathir, was an Arab Islamic exegete, historian and scholar. An expert on tafsir (Quranic exegesis), " +
                        "tarikh (history) and fiqh (Islamic jurisprudence), he is considered a leading authority on Sunni Islam.\r\n\r\nBorn in Bostra, Mamluk Sultanate, Ibn Kathir's " +
                        "teachers include al-Dhahabi and Ibn Taymiyya. He wrote several books, including a fourteen-volume universal history titled al-Bidaya wa'l-Nihaya (Arabic: البداية والنهاية)." +
                        "\r\n\r\nHis renowned tafsir, Tafsir Ibn Kathir, is recognized for its critical approach to Israʼiliyyat, especially among Western Muslims and Wahhabi scholars. " +
                        "His methodology largely derives from his teacher Ibn Taymiyya, and differs from that of other earlier renowned exegetes such as Tabari. He adhered to the Athari" +
                        " school of Islamic theology which rejected rationalistic theology in favor of strict textualism in the interpretation of the Quran and the hadith."
                    },
                    new Author
                    {
                        Id = "21850d05-c2a8-453b-9e6c-40d29b53571b",
                        Name = "Al-Ghazali",
                        Description = "Al-Ghazali, in Persian: ابو حامد محمد ابن محمد غزالی توسی, romanized: Abū Ḥāmid Muḥammad ibn Muḥammad Ghazālī Ṭūsi (c. 1058 – 19 December 1111), " +
                        "Latinized as Algazelus, was a Shafi'i Sunni Muslim Iranian scholar and polymath. He is known as one of the most prominent and influential jurisconsults," +
                        " legal theoreticians, muftis, philosophers, theologians, logicians and mystics in Islamic history.\r\n\r\nHe is considered to be the 11th century's mujaddid, " +
                        "a renewer of the faith, who, according to the prophetic hadith, appears once every 100 years to restore the faith of the Islamic community. Al-Ghazali's" +
                        " works were so highly acclaimed by his contemporaries that he was awarded the honorific title \"Proof of Islam\" (Ḥujjat al-Islām). Al-Ghazali was a " +
                        "prominent mujtahid in the Shafi'i school of law.\r\n\r\nMuch of Al-Ghazali's work stemmed around his spiritual crises following his appointment as the" +
                        " head of the Nizamiyya University in Baghdad, which was the most prestigious academic position in the Muslim world at the time. This led to his eventual " +
                        "disappearance from the Muslim world for over 10 years, realising he chose the path of status and ego over God. It was during this period where many of his" +
                        " great works were written. He believed that the Islamic spiritual tradition had become moribund and that the spiritual sciences taught by the first " +
                        "generation of Muslims had been forgotten. This belief led him to write his magnum opus entitled Iḥyā’ ‘ulūm ad-dīn (\"The Revival of the Religious Sciences\")." +
                        " Among his other works, the Tahāfut al-Falāsifa (\"Incoherence of the Philosophers\") is a landmark in the history of philosophy, as it advances the critique" +
                        " of Aristotelian science developed later in 14th-century Europe."
                    },
                    new Author
                    {
                        Id = "d58daee2-1ccb-4c81-bdcd-5620a0ff1ac1",
                        Name = "Shams Tabrizi",
                        Description = "Shams Tabrīzī (1185–1248) was a Persian dervish and poet, best known for his companionship with Rumi.\r\n\r\nHe is referenced with great" +
                        " reverence and grief in Rumi's poetic collection, in particular Divan-i Shams-i Tabrīzī. Tradition holds that Shams taught Rumi in seclusion in Konya" +
                        " for a period of forty days, before fleeing for Damascus. The tomb of Shams-i Tabrīzī was recently nominated to be a UNESCO World Heritage Site."
                    },
                    new Author
                    {
                        Id = "95b35c34-593f-4701-8f15-4a08c4108db7",
                        Name = "Primo Levi",
                        Description = "Primo Michele Levi (31 July 1919 – 11 April 1987) was a Italian chemist, partisan, Holocaust survivor and writer. He was the author of " +
                        "several books, collections of short stories, essays, poems and one novel. His best-known works include: If This Is a Man (Se questo è un uomo, 1947," +
                        " published as Survival in Auschwitz in the United States), his account of the year he spent as a prisoner in the Auschwitz concentration camp in " +
                        "Nazi-occupied Poland; and The Periodic Table (1975), a collection of mostly autobiographical short stories, each named after a chemical element which" +
                        " plays a role in each story, which the Royal Institution named the best science book ever written.\r\n\r\nLevi died in 1987 from injuries sustained" +
                        " in a fall from a third-storey apartment landing. His death was officially ruled a suicide, although that has been disputed by some of his friends " +
                        "and associates and attributed to an accident"
                    },
                    new Author
                    {
                        Id = "fcd3bf3a-5aaa-4f9d-a75a-f6da3809541b",
                        Name = "Ian Stewart",
                        Description = "Ian Nicholas Stewart (born 24 September 1945) is a British mathematician and a popular-science and science-fiction writer. " +
                        "He is Emeritus Professor of Mathematics at the University of Warwick, England.\r\nStewart was born in 1945 in Folkestone, England. While in the sixth form " +
                        "at Harvey Grammar School in Folkestone he came to the attention of the mathematics teacher. The teacher had Stewart sit mock A-level examinations without" +
                        " any preparation along with the upper-sixth students; Stewart was placed first in the examination. He was awarded a scholarship to study at the University" +
                        " of Cambridge as an undergraduate student of Churchill College, Cambridge, where he studied the Mathematical Tripos and obtained a first-class Bachelor of Arts" +
                        " degree in mathematics in 1966. Stewart then went to the University of Warwick where his PhD on Lie algebras was supervised by Brian Hartley and completed in 1969."
                    },
                    new Author
                    {
                        Id = "7d265cb9-77bf-47db-b2b9-468a3441d2d9",
                        Name = "Jabir Ibn Hayyan",
                        Description = "Abū Mūsā Jābir ibn Ḥayyān (Arabic: أَبو موسى جابِر بِن حَيّان, variously called al-Ṣūfī, al-Azdī, al-Kūfī, or al-Ṭūsī), died c. 806−816, is the purported" +
                        " author of a large number of works in Arabic, often called the Jabirian corpus. The c. 215 treatises that survive today mainly deal with alchemy and chemistry, " +
                        "magic, and Shi'ite religious philosophy. However, the original scope of the corpus was vast, covering a wide range of topics ranging from cosmology, astronomy " +
                        "and astrology, over medicine, pharmacology, zoology and botany, to metaphysics, logic, and grammar.\r\n\r\nThe works attributed to Jabir, which are tentatively " +
                        "dated to c. 850 – c. 950, contain the oldest known systematic classification of chemical substances, and the oldest known instructions for deriving an inorganic " +
                        "compound (sal ammoniac or ammonium chloride) from organic substances (such as plants, blood, and hair) by chemical means. His works also contain one of the " +
                        "earliest known versions of the sulfur-mercury theory of metals, a mineralogical theory that would remain dominant until the 18th century.\r\n\r\nA significant " +
                        "part of Jabir's writings deal with a philosophical theory known as \"the science of the balance\" (Arabic: ʿilm al-mīzān), which was aimed at reducing all " +
                        "phenomena (including material substances and their elements) to a system of measures and quantitative proportions. The Jabirian works also contain some of the" +
                        " earliest preserved Shi'ite imamological doctrines, which Jabir presented as deriving from his purported master, the Shi'ite Imam Jaʿfar al-Ṣādiq (died 765)." +
                        "\r\n\r\nAs early as the 10th century, the identity and exact corpus of works of Jabir was in dispute in Islamic scholarly circles. The authorship of all these" +
                        " works by a single figure, and even the existence of a historical Jabir, are also doubted by modern scholars. Instead, Jabir ibn Hayyan is generally thought to" +
                        " have been a pseudonym used by an anonymous school of Shi'ite alchemists writing in the late 9th and early 10th centuries.\r\n\r\nSome Arabic Jabirian works " +
                        "(e.g., The Great Book of Mercy, and The Book of Seventy) were translated into Latin under the Latinized name Geber, and in 13th-century Europe an anonymous writer," +
                        " usually referred to as pseudo-Geber, started to produce alchemical and metallurgical writings under this name"
                    },
                    new Author
                    {
                        Id = "2820b458-531b-4db4-abb2-99cd3e50c3b4",
                        Name = "Isaac Newton",
                        Description = "Sir Isaac Newton (4 January [O.S. 25 December] 1643 – 31 March [O.S. 20 March] 1727) was an English polymath who was a mathematician, physicist, " +
                        "astronomer, alchemist, theologian, author and inventor. He was a key figure in the Scientific Revolution and the Enlightenment that followed. His book Philosophiæ" +
                        " Naturalis Principia Mathematica (Mathematical Principles of Natural Philosophy), first published in 1687, achieved the first great unification in physics and " +
                        "established classical mechanics. Newton also made seminal contributions to optics, and shares credit with the German mathematician Gottfried Wilhelm Leibniz for" +
                        " formulating infinitesimal calculus, although he developed calculus years before Leibniz. Newton contributed to and refined the scientific method, and his work is" +
                        " considered the most influential in bringing forth modern science.\r\n\r\nIn the Principia, Newton formulated the laws of motion and universal gravitation that " +
                        "formed the dominant scientific viewpoint for centuries until it was superseded by the theory of relativity. While this is the case, his laws still serve as excellent" +
                        " approximations for the vast majority of physical phenomena involving low speeds (much less than the speed of light) and weak gravitational fields. " +
                        "He used his mathematical description of gravity to derive Kepler's laws of planetary motion, account for tides, the trajectories of comets, the precession of the " +
                        "equinoxes and other phenomena, eradicating doubt about the Solar System's heliocentricity. Newton solved the two-body problem and introduced the three-body problem." +
                        " He demonstrated that the motion of objects on Earth and celestial bodies could be accounted for by the same principles. Newton's inference that the Earth is an " +
                        "oblate spheroid was later confirmed by the geodetic measurements of Alexis Clairaut, Charles Marie de La Condamine, and others, convincing most European scientists" +
                        " of the superiority of Newtonian mechanics over earlier systems. He was also the first to calculate the age of Earth by experiment, and described a precursor to " +
                        "the modern wind tunnel. Further, he was the first to provide a quantitative estimate of the solar mass.\r\n\r\nNewton built the first reflecting telescope and " +
                        "developed a sophisticated theory of colour based on the observation that a prism separates white light into the colours of the visible spectrum. His work on light" +
                        " was collected in his book Opticks, published in 1704. He originated prisms as beam expanders and multiple-prism arrays, which would later become integral to the " +
                        "development of tunable lasers. He anticipated the wave–particle duality of light and was first to theorise the Goos–Hänchen effect. He also formulated an " +
                        "empirical law of cooling, which was the first heat transfer formulation and serves as the formal basis of convective heat transfer, made the first theoretical " +
                        "calculation of the speed of sound, and introduced the notions of a Newtonian fluid and a black body. He was also the first to explain the Magnus effect. Moreover, " +
                        "he was the first to analyse Couette flow. In addition to his creation of calculus, Newton's work on mathematics was extensive. He generalised the binomial theorem " +
                        "to any real number, introduced the Puiseux series, was the first to state Bézout's theorem, classified most of the cubic plane curves, contributed to the study of " +
                        "Cremona transformations, developed a method for approximating the roots of a function, originated the Newton–Cotes formulas used for numerical integration, and " +
                        "further produced the earliest explicit enunciation of the general Taylor series. Additionally, Newton initiated the field of calculus of variations, formulated " +
                        "and solved the earliest problem in geometric probability, devised the earliest form of linear regression, and was a pioneer of vector analysis.\r\n\r\nNewton was " +
                        "a fellow of Trinity College and the second Lucasian Professor of Mathematics at the University of Cambridge; he was appointed at the age of 26. He was a devout but" +
                        " unorthodox Christian who privately rejected the doctrine of the Trinity. He refused to take holy orders in the Church of England, unlike most members of the " +
                        "Cambridge faculty of the day. Beyond his work on the mathematical sciences, Newton dedicated much of his time to the study of alchemy and biblical chronology, " +
                        "but most of his work in those areas remained unpublished until long after his death. Politically and personally tied to the Whigs, Newton served two brief terms" +
                        " as Member of Parliament for the University of Cambridge, in 1689–1690 and 1701–1702. He was knighted by Queen Anne in 1705 and spent the last three decades of " +
                        "his life in London, serving as Warden (1696–1699) and Master (1699–1727) of the Royal Mint, in which he increased the accuracy and security of British coinage. " +
                        "He was also the president of the Royal Society (1703–1727)."
                    },
                    new Author
                    {
                        Id = "e339256a-dfc3-45bb-b5c3-4bf9f3526058",
                        Name = "Socrates",
                        Description = "Socrates ( 470 – 399 BC) was an ancient Greek philosopher from Classical Athens, perhaps the first Western moral philosopher, and a major inspiration" +
                        " on his student Plato, who largely founded the tradition of Western philosophy. An enigmatic figure, Socrates authored no texts and is known mainly through the" +
                        " posthumous accounts of classical writers, particularly his students Plato and Xenophon. These accounts are written as dialogues, in which Socrates and his " +
                        "interlocutors examine a subject in the style of question and answer; they gave rise to the Socratic dialogue literary genre. Contradictory accounts of Socrates " +
                        "make a reconstruction of his philosophy nearly impossible, a situation known as the Socratic problem. Socrates was a polarizing figure in Athenian society. " +
                        "In 399 BC, he was accused of impiety and corrupting the youth. After a trial that lasted a day, he was sentenced to death. As related by Plato, he was put to death" +
                        " by administration of poison after refusing offers from allies to help him escape.\r\n\r\nPlato's dialogues are among the most comprehensive accounts of Socrates " +
                        "to survive from antiquity. They demonstrate the Socratic approach to areas of philosophy including epistemology and ethics. The Platonic Socrates lends his name" +
                        " to the concept of the Socratic method, and also to Socratic irony. The Socratic method of questioning, or elenchus, takes shape in dialogue using short questions" +
                        " and answers, epitomized by those Platonic texts in which Socrates and his interlocutors examine various aspects of an issue or an abstract meaning, usually " +
                        "relating to one of the virtues, and find themselves at an impasse, unable to define what they thought they understood. Socrates frequently proclaims his ignorance," +
                        " saying that he is only sure that he does not know.\r\n\r\nSocrates exerted a strong influence on philosophers in later antiquity and has continued to do so in the" +
                        " modern era. He was studied by medieval and Islamic scholars and played an important role in the thought of the Italian Renaissance, particularly within the " +
                        "humanist movement. Interest in him continued unabated, as reflected in the works of Søren Kierkegaard and Friedrich Nietzsche. Depictions of Socrates in art, " +
                        "literature, and popular culture have made him a widely known figure in the Western philosophical tradition."
                    },
                    new Author
                    {
                        Id = "6d12378e-21fa-451d-a6d8-c39e4a42c8fa",
                        Name = "Plato",
                        Description = "Plato (born c. 428–423 BC, died 348/347 BC) was an ancient Greek philosopher of Classical Athens who is most commonly considered the foundational " +
                        "thinker of the Western philosophical tradition. An innovator of the literary dialogue and dialectic forms, Plato influenced all the major areas of theoretical" +
                        " philosophy and practical philosophy, and was the founder of the Platonic Academy, a philosophical school in Athens where Plato taught the collection of " +
                        "philosophical theories that would later become known as Platonism.\r\n\r\nPlato's most famous contribution is his Theory of Forms (or Ideas), which aims to " +
                        "solve what is now known as the problem of universals. He was influenced by the pre-Socratic thinkers Pythagoras, Heraclitus, and Parmenides, although much " +
                        "of what is known about them is derived from Plato himself.\r\n\r\nAlong with his teacher Socrates, and his student Aristotle, Plato is a central figure in the " +
                        "history of Western philosophy. Plato's complete works are believed to have survived for over 2,400 years—unlike that of nearly all of his contemporaries. " +
                        "Although their popularity has fluctuated, they have consistently been read and studied through the ages. Through Platonism's outgrowth Neoplatonism, " +
                        "he also influenced Christian philosophy, and both Jewish and Islamic philosophy. In modern times, Alfred North Whitehead said: \"the safest general " +
                        "characterization of the European philosophical tradition is that it consists of a series of footnotes to Plato.\""
                    },
                    new Author
                    {
                        Id = "5d32e53f-134b-4c07-a870-61368bcf0dfc",
                        Name = "Aristotle",
                        Description = "Aristotle (384–322 BC) was an ancient Greek philosopher and polymath. His writings cover a broad range of subjects spanning the natural sciences," +
                        " philosophy, linguistics, economics, politics, psychology, and the arts. As the founder of the Peripatetic school of philosophy in the Lyceum in Athens, he began" +
                        " the wider Aristotelian tradition that followed, which set the groundwork for the development of modern science.\r\n\r\nLittle is known about Aristotle's life. " +
                        "He was born in the city of Stagira in northern Greece during the Classical period. His father, Nicomachus, died when Aristotle was a child, and he was brought " +
                        "up by a guardian. At around eighteen years old, he joined Plato's Academy in Athens and remained there until the age of thirty seven (c. 347 BC). Shortly after " +
                        "Plato died, Aristotle left Athens and, at the request of Philip II of Macedon, tutored his son Alexander the Great beginning in 343 BC. He established a library " +
                        "in the Lyceum, which helped him to produce many of his hundreds of books on papyrus scrolls.\r\n\r\nThough Aristotle wrote many treatises and dialogues for " +
                        "publication, only around a third of his original output has survived, none of it intended for publication. Aristotle provided a complex synthesis of the various " +
                        "philosophies existing prior to him. His teachings and methods of inquiry have had a significant impact across the world, and remain a subject of contemporary " +
                        "philosophical discussion.\r\n\r\nAristotle's views profoundly shaped medieval scholarship. The influence of his physical science extended from late antiquity " +
                        "and the Early Middle Ages into the Renaissance, and was not replaced systematically until the Enlightenment and theories such as classical mechanics were developed." +
                        " He influenced Judeo-Islamic philosophies during the Middle Ages, as well as Christian theology, especially the Neoplatonism of the Early Church and the scholastic" +
                        " tradition of the Catholic Church.\r\n\r\nAristotle was revered among medieval Muslim scholars as \"The First Teacher\", and among medieval Christians like Thomas " +
                        "Aquinas as simply \"The Philosopher\", while the poet Dante called him \"the master of those who know\". He has been referred to as the first scientist. His works" +
                        " contain the earliest known systematic study of logic, and were studied by medieval scholars such as Peter Abelard and Jean Buridan. His influence on logic " +
                        "continued well into the 19th century. In addition, his ethics, although always influential, has gained renewed interest with the modern advent of virtue ethics."
                    },
                    new Author
                    {
                        Id = "793f21bf-7f1c-48bb-ae64-0d56bbeacd12",
                        Name = "Bertrand Russell",
                        Description = "Bertrand Arthur William Russell, 3rd Earl Russell (18 May 1872 – 2 February 1970), was an English philosopher, logician, mathematician, " +
                        "and public intellectual. He influenced mathematics, logic, set theory, and various areas of analytic philosophy.\r\n\r\nHe was one of the early 20th " +
                        "century's prominent logicians and a founder of analytic philosophy, along with his predecessor Gottlob Frege, his friend and colleague G. E. Moore, " +
                        "and his student and protégé Ludwig Wittgenstein. Russell with Moore led the British \"revolt against idealism\". Together with his former teacher Alfred" +
                        " North Whitehead, Russell wrote Principia Mathematica, a milestone in the development of classical logic and a major attempt to reduce the whole of mathematics" +
                        " to logic (see logicism). Russell's article \"On Denoting\" has been considered a \"paradigm of philosophy\".\r\n\r\nRussell was educated at Trinity College " +
                        "at the University of Cambridge, where he graduated in 1893. He was a pacifist who championed anti-imperialism and chaired the England-based India League. " +
                        "He went to prison for his pacifism during the First World War, and he initially supported appeasing Adolf Hitler's Nazi Germany, before changing his view in 1943," +
                        " describing war as a necessary \"lesser of two evils\". In the wake of the Second World War, he welcomed American global hegemony in preference to either " +
                        "Soviet hegemony or no (or ineffective) world leadership, even if it were to come at the cost of using their nuclear weapons. He would later criticise Stalinist" +
                        " totalitarianism, condemn the United States' involvement in the Vietnam War, and become an outspoken proponent of nuclear disarmament.\r\n\r\nIn 1950 Russell" +
                        " was awarded the Nobel Prize in Literature \"in recognition of his varied and significant writings in which he champions humanitarian ideals and freedom of " +
                        "thought\". He was also the recipient of the De Morgan Medal (1932), Sylvester Medal (1934), Kalinga Prize (1957), and Jerusalem Prize (1963)."
                    },
                    new Author
                    {
                        Id = "6168d9bf-9ce6-421f-84cd-3fff95ba65b8",
                        Name = "Friedrich Nietzsche",
                        Description = "Friedrich Wilhelm Nietzsche (15 October 1844 – 25 August 1900) was a German philosopher who started his career as a classical" +
                        " philologist and turned to philosophy early in his academic career. In 1869, aged 24, he was appointed Professor of Classical Philology at the" +
                        " University of Basel. Plagued by health problems for most of his life, he resigned from the university in 1879. He afterward lived as an independent writer," +
                        " spending much of his life in relative solitude and financial insecurity while moving between Switzerland, Italy, and southern France in search of climates " +
                        "that might alleviate his condition, and in the following decade, he completed much of his core writing. In 1889, aged 44, he suffered a mental breakdown and" +
                        " thereafter a complete loss of his mental faculties, with paralysis and vascular dementia, living his remaining 11 years under the care of his family until" +
                        " his death. His works and his philosophy have fostered not only extensive scholarship but also much popular interest.\r\n\r\nNietzsche's work encompasses poetry," +
                        " cultural criticism, and philosophical essays while displaying a fondness for aphorisms and irony. Prominent elements of his philosophy include the response of the" +
                        " Übermensch; his critique of truth in favour of perspectivism; a genealogical critique of Christian morality and a related theory of master–slave morality; " +
                        "the affirmation of life in response to both the weakening of religion and the crisis of passive nihilism which followed it; the notion of Apollonian and " +
                        "Dionysian forces; and a characterisation of the human subject as the expression of competing wills, collectively understood as the Will-to-power.\r\n\r\nAfter" +
                        " Nietzsche's death, his sister, Elisabeth Förster-Nietzsche, became the curator and editor of his manuscripts. She edited his unpublished writings to fit her " +
                        "German ultranationalist ideology, often contradicting or obfuscating Nietzsche's stated opinions, which were explicitly opposed to antisemitism and nationalism." +
                        " Through her published editions, Nietzsche's work became associated with fascism and Nazism. Twentieth-century scholars such as Walter Kaufmann, R. J. Hollingdale" +
                        " and Georges Bataille defended Nietzsche against this interpretation, and corrected editions of his writings were soon made available. The works of Nietzsche then" +
                        " renewed popularity in the 1960s and his ideas have since had a widespread impact on 20th- and 21st-century thinkers across philosophy—especially in schools of " +
                        "continental philosophy such as existentialism, postmodernism and post-structuralism—as well as art, literature, music, poetry, politics and popular culture."
                    },
                    new Author
                    {
                        Id = "36cfd3ba-0b17-4a48-afb9-d7bdec093f43",
                        Name = "John McCarthy",
                        Description = "John McCarthy (September 4, 1927 – October 24, 2011) was an American computer scientist and cognitive scientist. " +
                        "He was one of the founders of the discipline of artificial intelligence, and part of just a small group of artificial intelligence " +
                        "researchers in the 1950s and 1960s. He co-authored the proposal for the Dartmouth workshop which coined the term \"artificial intelligence\" (AI), " +
                        "led the development of the symbolic programming language family Lisp and had a large influence in the language ALGOL, popularized time-sharing, " +
                        "and created garbage collection.\r\n\r\nMcCarthy spent most of his career at Stanford University. He received many accolades and honors, such as the 1971 " +
                        "Turing Award for his contributions to the topic of AI, the United States National Medal of Science, and the Kyoto Prize."
                    },
                    new Author
                    {
                        Id = "a03ef8b6-33f6-4fd4-93d9-8bea3d2e4f3b",
                        Name = "J. K. Rowling",
                        Description = "Joanne Rowling (born 31 July 1965), better known by her pen name J. K. Rowling, is the British novelist who wrote Harry Potter, " +
                        "a seven-volume series about a young wizard. Published from 1997 to 2007, the fantasy novels are the best-selling book series in history, " +
                        "with over 600 million copies sold. They have been translated into 84 languages and have spawned a global media franchise including films and " +
                        "video games. She writes Cormoran Strike, an ongoing crime fiction series, under the alias Robert Galbraith.\r\n\r\nBorn in Yate, Gloucestershire, " +
                        "Rowling was working as a researcher and bilingual secretary for Amnesty International in 1990 when she conceived the idea for the Harry Potter series." +
                        " The seven-year period that followed saw the death of her mother, the birth of her first child, divorce from her first husband, and relative poverty " +
                        "until the first novel in the series, Harry Potter and the Philosopher's Stone, was published in 1997. Six sequels followed, concluding with Harry Potter" +
                        " and the Deathly Hallows (2007). By 2008, Forbes had named her the world's highest-paid author.\r\n\r\nThe novels follow a boy called Harry Potter as he" +
                        " attends Hogwarts (a school for wizards), and battles Lord Voldemort. Death and the divide between good and evil are the central themes of the series. " +
                        "Its influences include Bildungsroman (the coming-of-age genre), school stories, fairy tales, and Christian allegory. The series revived fantasy as a genre in" +
                        " the children's market, spawned a host of imitators, and inspired an active fandom. Critical reception has been more mixed. Many reviewers see Rowling's writing" +
                        " as conventional; some regard her portrayal of gender and social division as regressive. There were also religious debates over the Harry Potter series." +
                        "\r\n\r\nRowling has won many accolades for her work. She was named to the Order of the British Empire and was appointed a member of the Order of the Companions" +
                        " of Honour for services to literature and philanthropy. Harry Potter brought her wealth and recognition, which she has used to advance philanthropic endeavours" +
                        " and political causes. She established the Volant Charitable Trust in 2000, and co-founded the charity Lumos in 2005. Rowling's philanthropy centres on medical" +
                        " causes and supporting at-risk women and children. In 2025, Forbes estimated that Rowling's charitable giving exceeded US$200 million. She has also donated to" +
                        " the British Labour Party, and opposed Scottish independence and Brexit.\r\n\r\nFrom 2019, Rowling began making public remarks about transgender people, opposing" +
                        " attempts to replace the legal definition of birth sex with gender self identity. She has been condemned as transphobic by LGBTQ rights groups and various other" +
                        " critics, including academics. This has affected her public image and relationship with readers and colleagues, altering the way they engage with her works."
                    },
                    new Author
                    {
                        Id = "2c4cf33a-c18d-46f3-ac91-e25204cadf64",
                        Name = "Geoffrey Hinton",
                        Description = "Geoffrey Everest Hinton (born 6 December 1947) is a British-Canadian computer scientist, cognitive scientist, and cognitive psychologist " +
                        "known for his work on artificial neural networks, which earned him the title \"the Godfather of AI\".\r\n\r\nHinton is University Professor Emeritus at " +
                        "the University of Toronto. From 2013 to 2023, he divided his time working for Google Brain and the University of Toronto before publicly announcing his " +
                        "departure from Google in May 2023, citing concerns about the many risks of artificial intelligence (AI) technology. In 2017, he co-founded and became the" +
                        " chief scientific advisor of the Vector Institute in Toronto.\r\n\r\nWith David Rumelhart and Ronald J. Williams, Hinton was co-author of a highly cited paper" +
                        " published in 1986 that popularised the backpropagation algorithm for training multi-layer neural networks, although they were not the first to propose the" +
                        " approach. Hinton is viewed as a leading figure in the deep learning community. The image-recognition milestone of the AlexNet designed in collaboration with" +
                        " his students Alex Krizhevsky and Ilya Sutskever for the ImageNet challenge 2012 was a breakthrough in the field of computer vision.\r\n\r\nHinton received " +
                        "the 2018 Turing Award, together with Yoshua Bengio and Yann LeCun for their work on deep learning. They are sometimes referred to as the \"Godfathers of " +
                        "Deep Learning\" and have continued to give public talks together. He was also awarded, along with John Hopfield, the 2024 Nobel Prize in Physics for " +
                        "\"foundational discoveries and inventions that enable machine learning with artificial neural networks\".\r\n\r\nIn May 2023, Hinton announced his " +
                        "resignation from Google to be able to \"freely speak out about the risks of A.I.\" He has voiced concerns about deliberate misuse by malicious actors," +
                        " technological unemployment, and existential risk from artificial general intelligence. He noted that establishing safety guidelines will require " +
                        "cooperation among those competing in use of AI in order to avoid the worst outcomes. After receiving the Nobel Prize, he called for urgent research into" +
                        " AI safety to figure out how to control AI systems smarter than humans"
                    }
                );
        }
    }
}
