using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GMEPDesignTool
{
    public class Proposal
    {
        public string Id;
        public string ProjectId;
        public DateTime DateCreated;
        public string Type;
        public string EmployeeUsername;

        public Proposal(
            string id,
            string projectId,
            DateTime dateCreated,
            string type,
            string employeeUsername
        )
        {
            Id = id;
            ProjectId = projectId;
            DateCreated = dateCreated;
            Type = type;
            EmployeeUsername = employeeUsername;
        }
    }

    public class ProposalQuestion
    {
        public int Id;
        public string Question;
        public string QuestionType;
        public string VariableName;

        public ProposalQuestion(int id, string question, string questionType, string variableName)
        {
            Id = id;
            Question = question;
            QuestionType = questionType;
            VariableName = variableName;
        }
    }

    public class ProposalAnswer
    {
        public string Id;
        public string ProjectId;
        public int QuestionId;
        public string Answer;

        public ProposalAnswer(string id, string projectId, int questionId, string answer)
        {
            Id = id;
            ProjectId = projectId;
            QuestionId = questionId;
            Answer = answer;
        }
    }
}
