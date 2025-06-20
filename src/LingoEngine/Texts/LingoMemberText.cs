
using LingoEngine.Casts;
using LingoEngine.Members;
using LingoEngine.Primitives;
using LingoEngine.Texts.FrameworkCommunication;

namespace LingoEngine.Texts
{
    public class LingoMemberText : LingoMemberTextBase<ILingoFrameworkMemberText>, ILingoMemberText
    {
        public LingoMemberText(LingoCast cast, ILingoFrameworkMemberText frameworkMember, int numberInCast, string name = "", string fileName = "", LingoPoint regPoint = default) : base(LingoMemberType.Text,cast, frameworkMember, numberInCast, name, fileName, regPoint)
        {
        }

        protected override LingoMember OnDuplicate(int newNumber)
        {
            throw new NotImplementedException();
            //var clone = new LingoMemberText(_cast, _lingoFrameworkMember, newNumber, Name);
            //clone.Text = Text;
            //return clone;
        }
        /// <summary>
      
       

      
    }

}

 

