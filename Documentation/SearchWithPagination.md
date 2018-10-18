## Search with Pagination
here is a simple example on how you could implement a simple generic method with pagination

```csharp
    
    public class TableTreeSettings
    {
        public string Sort { get; set; }

        public string SortColumn { get; set; }

        public int PageSize { get; set; }

        public int TotalPages { get; set; }

        public int SelectedPage { get; set; }

        public string SearchText { get; set; }

        public object Result { get; set; }
    }
   
   /// this method is added in dbContxt
   public TableTreeSettings Search<T>(TableTreeSettings settings, Expression<Predicate<T>> match)
        {
            settings.SearchText = settings.SearchText ?? "";
            if (settings.SelectedPage <= 0)
                settings.SelectedPage = 1;
            if (settings.PageSize <= 0)
                settings.PageSize = 20;
            var data = this.Get<T>().Where(match).LoadChildren();
            if (!string.IsNullOrEmpty(settings.SortColumn))
            {
            /// SortColumn is only firstlevel property like UserName
                if (settings.Sort != "desc")
                    data = data.OrderBy(settings.SortColumn);
                else data = data.OrderByDescending(settings.SortColumn);
            }
            // we get the total rows before we add Skip and Take. 
            // ExecuteCount only execute select count(*) for the selected search or match rows
            settings.TotalPages = (data.ExecuteCount() / settings.PageSize).ConvertValue<int>();
            data = data.Skip(settings.SelectedPage * settings.PageSize).Take(settings.PageSize);
            settings.Result = data.Execute();
            return settings; // here after you could use the data to post it back as json or use it any other way
        }
        
        // we could call this method like 
        using (var rep = new dbContext()){
        // tableSettings is the one we get from the page eq posted json data 
        TableTreeSettings data = rep.Search<User>(tableSettings, x=> x.UserName.Contains(tableSettings.SearchText));
        
        }

```
