using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Text.RegularExpressions;
using UnityEngine;

namespace Checkers
{
    public class ObserverManager : MonoBehaviour, IWillRecordYourPlay
    {
        [SerializeField]
        public ObserverState CurrentState = ObserverState.None;
        [SerializeField]
        public float _secondsBetweenChooseChip = 0.5f;
        [SerializeField]
        public float _secondsBetweenMoveChip = 1f;

        private ICanPlayCheckers Checkers;

        public void SetCheckers(ICanPlayCheckers checkers)
        {
            Checkers = checkers;
        }

        private StreamWriter savetofile;
        private StreamReader readfile;
        private void OnEnable()
        {
            if (CurrentState == ObserverState.Write)
            {
                File.WriteAllText(@"Save.txt", string.Empty);
                savetofile = new StreamWriter("Save.txt", append: true);
            }
            if (CurrentState == ObserverState.Read)
            {
                readfile = new StreamReader("Save.txt");
                StartCoroutine(ReplayCoroutine());
            }
        }
        Regex regular = new Regex(@"(\w*):(\d[A-H])");
        IEnumerator ReplayCoroutine()
        {
            yield return new WaitForSeconds(1);
            float nextWait = _secondsBetweenMoveChip;
            while (!readfile.EndOfStream)
            {
                yield return new WaitForSeconds(nextWait);
                if (Checkers.AreYouBusy())
                {
                    nextWait = 0.1f;
                }
                else
                {
                    var line = readfile.ReadLine();
                    if (regular.IsMatch(line))
                    {
                        var match = regular.Match(line);
                        if (match.Groups[1].ToString() == "Choose")
                        {
                            Checkers.ChooseChip(match.Groups[2].ToString());
                            nextWait = _secondsBetweenChooseChip;
                        }
                        else
                        {
                            Checkers.MoveChip(match.Groups[2].ToString());
                            nextWait = _secondsBetweenMoveChip;
                        }
                    }
                }
            }
            Debug.Log("End of save");
        }

        private void OnDisable()
        {
            if (CurrentState == ObserverState.Write)
            {
                savetofile.Flush();
                savetofile.Close();
            }
            if (CurrentState == ObserverState.Read)
            {
                readfile.Close();
            }
        }

        public void SaveChoose(string chooseString)
        {
            if (CurrentState == ObserverState.Write)
            {
                savetofile.WriteLine("Choose:" + chooseString);
            }
        }

        public void SaveMove(string moveString)
        {
            if (CurrentState == ObserverState.Write)
            {
                savetofile.WriteLine("Move:" + moveString);
            }
        }
    }


    public enum ObserverState { None, Write, Read }


    public interface IWillRecordYourPlay
    {
        void SaveChoose(string chooseString);
        void SaveMove(string moveString);
    }
}