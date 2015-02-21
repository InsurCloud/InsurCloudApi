using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using CorPolicy;

namespace Helpers
{
    public static class UWQuestionHelper
    {
        public static bool UWQuestionAnsweredAffirmatively(List<clsUWQuestion> questions, string questionCode)
        {
            foreach (clsUWQuestion question in questions)
            {
                if (question.QuestionCode == questionCode && question.AnswerText.ToUpper().Substring(0, 3) == "YES")
                {
                    return true;
                }
            }
            return false;
        }
    }
}
