## Example of Many to Many Relationships
This is an example of how to use Many to Many Relationships in EntityWorker.Core
We will create two class Menus and Article 
```csharp
    public class Menus
    {
        [PrimaryKey]
        public long? Id { get; set; }

        [NotNullable]

        public string DisplayName { get; set; }

        [ForeignKey(typeof(Menus))]
        public long? ParentId { get; set; }


        /// <summary>
        /// Indicate 
        /// this is a list so the where sats will be 
        /// Select * from Menus where ParentId = Id
        /// the parentId in this list will be set to Id automaticly and this list will be children to the current Menus
        /// </summary>
        public List<Menus> Children { get; set; }

        [NotNullable]
        public string Uri { get; set; }

        public bool Publish { get; set; }

        public string Description { get; set; }

    }

    [Table("Articles")]
    public class Article
    {
        [PrimaryKey]
        public Guid? Id { get; set; }

        [NotNullable]
        public string ArticleName { get; set; }

        public bool Published { get; set; }

        // Its importend to se propertyName in Manytomany relations
        [ForeignKey(type: typeof(Menus), propertyName: "Menus")]
        public long MenusId { get; set; }
        
        // Referense to menus 
        [IndependentData]
        public Menus Menus { get; set; }

        [ForeignKey(typeof(Article))]
        public System.Guid? ArticleId { get; set; }

        // edited but not published yet
        public List<Article> ArticleTemp { get; set; }
    }
}

```
