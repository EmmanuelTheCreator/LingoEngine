
using LingoEngine.Core;
using LingoEngine.Primitives;

namespace LingoEngine.Texts
{

    public class LingoMemberField : LingoMemberTextBase<ILingoFrameworkMemberField>, ILingoMemberField
    {
      
        private bool _isFocused;


        #region Properties
       


        #endregion
        public bool IsFocused => _isFocused;


        public LingoMemberField(LingoCast cast, ILingoFrameworkMemberField frameworkMember, int numberInCast, string name = "", string fileName = "", LingoPoint regPoint = default) : base(LingoMemberType.Field, cast, frameworkMember, numberInCast, name, fileName, regPoint)
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



