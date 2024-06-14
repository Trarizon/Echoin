using UnityEngine;

namespace Echoin.ProjectModels
{
    public enum NoteGrade
    {
        None = 0,
        Miss,
        Bad,
        /// <summary>
        /// Hold在中途抬手，此时不记录分数但不断连
        /// </summary>
        HoldReleased,
        Great,
        Perfect
    }

    public static class NoteGradeExtensions
    {
        public static Color ToJudgeColor(this NoteGrade grade)
        {
            return grade switch {
                NoteGrade.Bad => new Color(0.4f, 0.9f, 1f),
                NoteGrade.HoldReleased => new Color(0.4f, 0.9f, 1f),
                NoteGrade.Great => new Color(1f, 0.98f, 0.66f),
                NoteGrade.Perfect => Color.white,
                _ => Color.white,
            };
        }
    }
}