using System;

namespace Checkers
{
    public enum Moves { UpToLeft, UpToRight, DownToLeft, DownToRight }
    public class BaseCoord : System.Object
    {
        public int PosI;
        public int PosJ;
        public BaseCoord(int i, int j)
        {
            PosI = i;
            PosJ = j;
        }

        public BaseCoord GetNext(Moves move)
        {
            BaseCoord toreturn = null;
            switch (move)
            {
                case Moves.UpToLeft:
                    {
                        if (PosI - 1 >= 0 && PosJ + 1 <= 7)
                        {
                            toreturn = new BaseCoord(PosI - 1, PosJ + 1);
                        }
                        break;
                    }
                case Moves.UpToRight:
                    {
                        if (PosI + 1 <= 7 && PosJ + 1 <= 7)
                        {
                            toreturn = new BaseCoord(PosI + 1, PosJ + 1);
                        }
                        break;
                    }
                case Moves.DownToLeft:
                    {
                        if (PosI - 1 >= 0 && PosJ - 1 >= 0)
                        {
                            toreturn = new BaseCoord(PosI - 1, PosJ - 1);
                        }
                        break;
                    }
                case Moves.DownToRight:
                    {
                        if (PosI + 1 <= 7 && PosJ - 1 >= 0)
                        {
                            toreturn = new BaseCoord(PosI + 1, PosJ - 1);
                        }
                        break;
                    }
                default:
                    {
                        break;
                    }
            }
            return toreturn;
        }

        public string GePositionName()
        {
            return (PosJ + 1).ToString() + Convert.ToChar(IntA() + PosI);
        }

        public static int IntA()
        {
            return Convert.ToInt32('A');
        }
        public static int Int1()
        {
            return Convert.ToInt32('1');
        }

        public override bool Equals(object obj)
        {
            if (obj != null && obj is BaseCoord anotherCoord)
            {
                if (this.PosI == anotherCoord.PosI &&
                    this.PosJ == anotherCoord.PosJ)
                {
                    return true;
                }
            }
            return false;
        }

        public override int GetHashCode()
        {
            int hashCode = -1086896647;
            hashCode = hashCode * -1521134295 + PosI.GetHashCode();
            hashCode = hashCode * -1521134295 + PosJ.GetHashCode();
            return hashCode;
        }

        public BaseCoord(string coord)
        {
            PosI = Convert.ToInt32(coord[1] - IntA());
            PosJ = Convert.ToInt32(Convert.ToInt32(coord[0]) - Int1());
        }
    }

}
