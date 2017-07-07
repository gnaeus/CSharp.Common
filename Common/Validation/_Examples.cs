using System.ComponentModel.DataAnnotations;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using Common.Validation;

partial class _Examples
{
    class UserModel
    {
        [Required]
        public string Login { get; set; }

        [StringLength(20)]
        public string Name { get; set; }
    }

    class UserValidator
    {
        public void PrintModelValidation(UserModel userModel)
        {
            foreach (ValidationError error in userModel.ValidateAnnotations())
            {
                Debug.Write($"{error.PropertyPath} : {error.ErrorCode} - {error.ErrorMessage}");
            }
            // Login : Required - The field Login is required
            // Name : MaxLength - The field Name must be a string with a maximum length of 20
        }

        public void PrintArgumentValidation(UserModel userModel)
        {
            foreach (ValidationError error in userModel.ValidateAnnotations(nameof(userModel)))
            {
                Debug.Write($"{error.PropertyPath} : {error.ErrorCode} - {error.ErrorMessage}");
            }
            // userModel.Login : Required - The field Login is required
            // userModel.Name : MaxLength - The field Name must be a string with a maximum length of 20
        }
    }

    class Article : IEnumerableValidatable
    {
        [Required]
        public string Title { get; set; }

        [StringLength(10000)]
        public string Content { get; set; }

        public IEnumerable<ValidationError> Validate()
        {
            if (!Content.Contains("Copyright ©"))
            {
                yield return new ValidationError(
                    nameof(Content), "MustContainCopyright",
                    "The field Content must contain Copyright ©");
            }
        }
    }

    public class Episode
    {
        public int Id { get; set; }
        public int Number { get; set; }
        public int[] ActorIds { get; set; }

        [StringLength(40)]
        public string Title { get; set; }
    }

    class TvSeries : IContextValidatable
    {
        public int Id { get; set; }
        public List<Episode> Episodes { get; set; }

        public void Validate(IValidationContext validationContext)
        {
            for (int i = 0; i < Episodes.Count; i++)
            {
                var episode = Episodes[i];

                if (Episodes.FirstOrDefault(e => e.Title == episode.Title) != episode)
                {
                    validationContext.AddError(
                        $"[{i}].{nameof(episode.Title)}", "NonUniqueTitle",
                        $"Episode {episode.Number} has non unique Title field");
                }
            }
        }
    }
    
    public class Actor
    {
        public int Id { get; set; }
        public int Name { get; set; }
    }

    class DbContext
    {
        public ICollection<TvSeries> Series { get; set; }
        public ICollection<Episode> Episodes { get; set; }
        public ICollection<Actor> Actors { get; set; }

        public void SaveChanges() { }
    }

    class TvSeriesService
    {
        readonly DbContext _dbContext;
        readonly IValidationContext _validationContext;
        
        public void SaveTvSeries(TvSeries tvSeries)
        {
            _validationContext.ValidateAnnotations(tvSeries);
            _validationContext.ThrowIfHasErrors();
            // Episodes[0].Title : StringLength - The field Ttile must be a string with a maximum length of 40
            // Episodes[1].Title : NonUniqueTitle - Episode 1 has non unique Title field

            _dbContext.Series.Add(tvSeries);

            for (int i = 0; i < tvSeries.Episodes.Count; i++)
            {
                using (_validationContext.WithPrefix($"Episodes[{i}]"))
                {
                    SaveEpisode(tvSeries.Episodes[i]);
                }
            }

            _validationContext.ThrowIfHasErrors();
            // Episodes[0].ActorIds : HasUnknownActors - Episode 0 has unknown actors
            // Episodes[1].ActorIds : HasUnknownActors - Episode 1 has unknown actors

            _dbContext.SaveChanges();
        }

        private void SaveEpisode(Episode episode)
        {
            int foundActorsCount = _dbContext.Actors
                .Select(a => a.Id)
                .Intersect(episode.ActorIds)
                .Count();

            if (foundActorsCount != episode.ActorIds.Length)
            {
                _validationContext.AddError(
                    nameof(episode.ActorIds), "HasUnknownActors",
                    $"Episode {episode.Number} has unknown actors");
            }

            _dbContext.Episodes.Add(episode);
        }
    }
}
