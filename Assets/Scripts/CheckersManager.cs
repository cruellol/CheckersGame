using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.UI;

namespace Checkers
{
    public class CheckersManager : MonoBehaviour , ICanPlayCheckers
    {
        [SerializeField]
        private GameObject _cellPrefab=null;
        [SerializeField]
        private GameObject _chipPrefab = null;
        [SerializeField]
        private GameObject _camera = null;
        [SerializeField]
        private Material _cellBlack = null;
        [SerializeField]
        private Material _cellWhite = null;
        [SerializeField]
        private Material _chipBlack = null;
        [SerializeField]
        private Material _chipWhite = null;
        [SerializeField]
        private Text _whoMovesText = null;
        [SerializeField]
        private Text _whiteChipsText = null;
        [SerializeField]
        private Text _blackChipsText = null;
        [SerializeField]
        private float _secondsToSwitchSide=0.5f;
        [SerializeField]
        private float _secondsToMoveChip=0.1f;

        /// <summary>
        ///Флаг, в движении ли сейчас
        /// </summary>
        private bool _inMove = false;
        /// <summary>
        ///Список фишек, которые должны съесть фигуру противнка на этом ходу
        /// </summary>
        private List<Chip> _hungryChips = new List<Chip>();
        private Chip _chosenChip = null;
        /// <summary>
        ///флаг, можем ли мы поменять текущую фишку
        /// </summary>
        private bool _chipCanChange = true;
        /// <summary>
        ///Список ходов, которые можем совершить выбранной фишкой, в случае атаки с ссылкой на съедаемую фишку противника
        /// </summary>
        private List<(Cell CellToMove, Chip ChipToEat)> _moveList = new List<(Cell CellToMove, Chip ChipToEat)>();
        private List<Cell> _allCells = new List<Cell>();
        private List<Chip> _allChips = new List<Chip>();

        private ObserverManager _currentObserver;

        private bool whiteMove = true;
        private bool _whiteMove
        {
            get
            {
                return whiteMove;
            }
            set
            {
                if (whiteMove != value)
                {
                    whiteMove = value;
                    StartCoroutine(SwitchSide());
                    ResetTexts();
                }
            }
        }

        private void ResetTexts()
        {
            if (_whiteMove)
            {
                _whoMovesText.text = "Ходят белые";
            }
            else
            {
                _whoMovesText.text = "Ходят черные";
            }
            int whitecount = 0;
            int blackcount = 0;
            foreach (var chip in _allChips)
            {
                if (chip.IsWhite) whitecount++;
                else blackcount++;
            }
            _whiteChipsText.text = "Фишек белых:" + whitecount;
            _blackChipsText.text = "Фишек черных:" + blackcount;
            if (whitecount == 0)
            {
                _whoMovesText.text = "Черные победили";
            }
            if (blackcount == 0)
            {
                _whoMovesText.text = "Белые победили";
            }
        }

        private Dictionary<bool, List<Moves>> _legitMoves = new Dictionary<bool, List<Moves>>() {
            {true,new List<Moves>(){ Moves.UpToLeft,Moves.UpToRight} },
            {false,new List<Moves>(){ Moves.DownToLeft,Moves.DownToRight} }
        };

        private void Start()
        {
            if (_cellPrefab != null && _cellBlack != null && _cellWhite != null)
            {
                CreateMap();
                CreateNames();
            }
            if (_chipPrefab != null && _chipBlack != null && _chipWhite != null)
            {
                CreateChips();
            }
            ResetTexts();

            _currentObserver = GetComponent<ObserverManager>();
            _currentObserver?.SetCheckers(this);
            if (_currentObserver != null)
            {
                if(_currentObserver._secondsBetweenChooseChip<= _secondsToMoveChip)
                {
                    _secondsToMoveChip = _currentObserver._secondsBetweenChooseChip * 0.9f;
                }
                if (_currentObserver._secondsBetweenMoveChip <= _secondsToSwitchSide)
                {
                    _secondsToSwitchSide = _currentObserver._secondsBetweenMoveChip * 0.9f;
                }
            }
        }


        public void RestartGame()
        {
            if (_currentObserver != null && _currentObserver.CurrentState != ObserverState.None)
            {
                Debug.Log("Не доступно в данном режиме observer-a");
                return;
            }
            _whiteMove = true;
            _inMove = false;
            _hungryChips.Clear();
            _chipCanChange = true;
            _chosenChip = null;
            _moveList.Clear();
            _allChips.ForEach(c => c.Destroy());
            _allChips.Clear();
            if (_chipPrefab != null && _chipBlack != null && _chipWhite != null)
            {
                CreateChips();
            }
            ResetTexts();
        }

        private void CreateChips()
        {
            for (int i = 0; i < 8; i++)
            {
                for (int j = 0; j < 8; j++)
                {
                    if ((i + j) % 2 == 0)
                    {
                        if (j <= 2 || j >= 5)
                        {
                            var bindRect = GameObject.Instantiate(_chipPrefab);
                            var actualChip = bindRect.transform.GetChild(0).gameObject;
                            var mesh = actualChip.GetComponent<MeshRenderer>();
                            bool iswhite = true;
                            if (j <= 2)
                            {
                                mesh.material = _chipWhite;
                            }
                            else
                            {
                                iswhite = false;
                                mesh.material = _chipBlack;
                            }
                            bindRect.transform.parent = gameObject.transform;
                            bindRect.transform.position = new Vector3(i, 0, j);
                            var newChip = new Chip(i, j, actualChip, bindRect, mesh.material, iswhite);
                            newChip.MouseEvent += ChipEvent;
                            _allChips.Add(newChip);
                        }
                    }
                }
            }
        }

        private void CreateMap()
        {
            for (int i = 0; i < 8; i++)
            {
                for (int j = 0; j < 8; j++)
                {
                    var actualCell = GameObject.Instantiate(_cellPrefab);
                    var mesh = actualCell.GetComponent<MeshRenderer>();
                    if ((i + j) % 2 == 0)
                    {
                        mesh.material = _cellBlack;
                    }
                    else
                    {
                        mesh.material = _cellWhite;
                    }
                    actualCell.transform.parent = gameObject.transform;
                    actualCell.transform.position = new Vector3(i, 0, j);
                    var newCell = new Cell(i, j, actualCell, mesh.material);
                    newCell.MouseEvent += CellEvent;
                    _allCells.Add(newCell);
                }
            }
        }

        private void CreateNames()
        {
            for (int i = 0; i < 8; i++)
            {
                var newtext = GetLetter((i + 1).ToString());
                newtext.transform.position = new Vector3(-1, 0, i);
            }

            for (int i = 0; i < 8; i++)
            {
                var newtext = GetLetter(Convert.ToChar(BaseCoord.IntA()+ i).ToString());
                newtext.transform.position = new Vector3(i, 0, -1);
            }

            for (int i = 0; i < 8; i++)
            {
                var newtext = GetLetter((i + 1).ToString());
                newtext.transform.position = new Vector3(8, 0, i);
                newtext.transform.Rotate(new Vector3(0, 0, 180));
            }

            for (int i = 0; i < 8; i++)
            {
                var newtext = GetLetter(Convert.ToChar(BaseCoord.IntA() + i).ToString());
                newtext.transform.position = new Vector3(i, 0, 8);
                newtext.transform.Rotate(new Vector3(0, 0, 180));
            }
        }        

        private GameObject GetLetter(string text)
        {
            var newTextObject = new GameObject();
            newTextObject.transform.localScale = new Vector3(0.07f, 0.07f, 0.07f);
            newTextObject.transform.Rotate(new Vector3(90, 0, 0));
            var textMesh = (TextMesh)newTextObject.AddComponent(typeof(TextMesh));
            //var meshRenderer = newTextObject.AddComponent(typeof(MeshRenderer));
            textMesh.text = text;
            textMesh.fontSize = 120;
            textMesh.anchor = TextAnchor.MiddleCenter;
            return newTextObject;
        }

        private Cell GetCell(BaseCoord toFind)
        {
            return _allCells.FirstOrDefault(c => c.Position.Equals(toFind));
        }
        private Chip GetChip(BaseCoord toFind)
        {
            return _allChips.FirstOrDefault(c => c.Position.Equals(toFind));
        }

        private void CellEvent(object sender, MouseEvents mouseEvent)
        {
            if (_currentObserver != null && _currentObserver.CurrentState == ObserverState.Read) return;
            if (_inMove) return;
            var cellinfo = sender as Cell;
            if (_moveList.Count == 0) return;
            var found = _moveList.FirstOrDefault(ctm => ctm.CellToMove == cellinfo);
            if (found.CellToMove != null)
            {
                switch (mouseEvent)
                {
                    case MouseEvents.Click:
                        {
                            MoveChipToString(found);
                            break;
                        }
                    case MouseEvents.Enter:
                        {
                            found.CellToMove.Emit(ToEmit.Blue);
                            break;
                        }
                    case MouseEvents.Leave:
                    default:
                        {
                            found.CellToMove.Emit(ToEmit.Green);
                            break;
                        }
                }
            }
        }

        private void ChipEvent(object sender, MouseEvents mouseEvent)
        {
            if (_currentObserver!=null && _currentObserver.CurrentState == ObserverState.Read) return;
            var chipInfo = sender as Chip;
            if (_inMove) return;
            if (chipInfo == _chosenChip) return;
            bool selectedHungry = IsHungry(chipInfo);
            if (_hungryChips.Count > 0 && !selectedHungry) return;
            if (_whiteMove == chipInfo.IsWhite && _chipCanChange)
            {
                switch (mouseEvent)
                {
                    case MouseEvents.Click:
                        {
                            ChooseChipToString(chipInfo);
                            break;
                        }
                    case MouseEvents.Enter:
                        {
                            if (_hungryChips.Count > 0)
                            {
                                if (selectedHungry)
                                {
                                    chipInfo.Emit(ToEmit.Green);
                                }
                            }
                            else
                            {
                                chipInfo.Emit(ToEmit.Green);
                            }
                            break;
                        }
                    case MouseEvents.Leave:
                    default:
                        {
                            if (selectedHungry)
                            {
                                chipInfo.Emit(ToEmit.Red);
                            }
                            else
                            {
                                chipInfo.Emit(ToEmit.None);
                            }
                            break;
                        }
                }
            }
        }

        public void ChooseChipToString(Chip chipInfo)
        {
            var chooseString=chipInfo.Position.GePositionName();
            
            //write in observer
            _currentObserver?.SaveChoose(chooseString);

            ChooseChip(chooseString);
        }

        public void ChooseChip(string chipInfoString)
        {
            BaseCoord newcoord=new BaseCoord(chipInfoString);
            var chip = GetChip(newcoord);
            ChooseChipBuffered(chip);
        }

        public void ChooseChipBuffered(Chip chipInfo)
        {
            Debug.Log(chipInfo.IsItWhiteString() +" " +chipInfo.Position.GePositionName());

            if (_chosenChip != null)
            {
                ResetAllEmit();
                if (IsHungry(_chosenChip))
                {
                    _chosenChip.Emit(ToEmit.Red);
                }
            }
            _chosenChip = chipInfo;
            if (IsHungry(chipInfo))
            {
                _moveList = OnlyToEat(_chosenChip);
            }
            else
            {
                _moveList = WhereCanMove(_chosenChip);
            }
            _moveList.ForEach(c => c.CellToMove.Emit(ToEmit.Green));
            chipInfo.Emit(ToEmit.Blue);
        }   
        
        public void MoveChipToString((Cell CellToMove, Chip ChipToEat) move)
        {
            var moveString = move.CellToMove.Position.GePositionName();

            //save to observer
            _currentObserver?.SaveMove(moveString);

            MoveChip(moveString);
        }

        public void MoveChip(string stringmove)
        {
            BaseCoord newcoord = new BaseCoord(stringmove);
            (Cell CellToMove, Chip ChipToEat) move =_moveList.FirstOrDefault(ctm => ctm.CellToMove.Position.Equals(newcoord));
            MoveChipBuffered(move);
        }

        public void MoveChipBuffered((Cell CellToMove, Chip ChipToEat) move)
        {
            Debug.Log(_chosenChip.IsItWhiteString() + " moves " + move.CellToMove.Position.GePositionName());

            StartCoroutine(MoveChipCoroutine(move));
        }

        IEnumerator MoveChipCoroutine((Cell CellToMove, Chip ChipToEat) move)
        {
            ResetAllEmit();
            _inMove = true;
            var idist = move.CellToMove.Position.PosI - _chosenChip.Position.PosI;
            var jdist = move.CellToMove.Position.PosJ - _chosenChip.Position.PosJ;
            var imove = (idist) > 0 ? 1 : -1;
            var jmove = (jdist) > 0 ? 1 : -1;

            int countMoveAnimation = 50;
            float SecondsToAnimate = _secondsToMoveChip / countMoveAnimation;
            float moveweight = Mathf.Abs(idist) / (float)countMoveAnimation;
            var lastpos = _chosenChip.BindRectObject.transform.position;

            for (int i = 0; i <= countMoveAnimation; i++)
            {
                yield return new WaitForSeconds(SecondsToAnimate);
                var JumpCoord = Mathf.Sin((Mathf.PI / countMoveAnimation * i)) * 0.5f;
                _chosenChip.BindRectObject.transform.position = lastpos + new Vector3(moveweight * imove * i, JumpCoord, moveweight * jmove * i);
            }

            _chosenChip.BindRectObject.transform.position = new Vector3(move.CellToMove.Position.PosI, 0, move.CellToMove.Position.PosJ);

            if (move.ChipToEat != null)
            {
                var chiptoeat = move.ChipToEat;
                chiptoeat.Destroy();
                _allChips.Remove(chiptoeat);
                ResetTexts();
            }

            ChangeChipPlace(move);

            bool switchingside = true;
            if (move.ChipToEat != null)
            {
                _moveList = OnlyToEat(_chosenChip);
                if (_moveList.Count > 0)
                {
                    _hungryChips.ForEach(h => h.Emit(ToEmit.None));
                    _moveList.ForEach(c => c.CellToMove.Emit(ToEmit.Green));
                    _chosenChip.Emit(ToEmit.Blue);
                    switchingside = false;
                    _chipCanChange = false;
                    _inMove = false;
                }
            }
            if (switchingside)
            {
                _whiteMove = !_whiteMove;
            }
        }

        private void ChangeChipPlace((Cell CellToMove, Chip ChipToEat) move)
        {
            _chosenChip.SetNewPos(new BaseCoord(move.CellToMove.Position.PosI, move.CellToMove.Position.PosJ));
            if (_chosenChip.IsWhite)
            {
                if (move.CellToMove.Position.PosJ == 7)
                {
                    _chosenChip.SetQueen();
                }
            }
            else
            {
                if (move.CellToMove.Position.PosJ == 0)
                {
                    _chosenChip.SetQueen();
                }
            }
        }

        IEnumerator SwitchSide()
        {
            _hungryChips.ForEach(h => h.Emit(ToEmit.None));

            int countRotationAnimation = 100;
            float SecondsToAnimate = _secondsToSwitchSide / countRotationAnimation;
            float moveweight = 180f / countRotationAnimation;

            for (int i = 0; i < countRotationAnimation; i += 1)
            {
                _camera.transform.RotateAround(new Vector3(3.5f, 5, 3.5f), new Vector3(0, 1, 0), moveweight);
                yield return new WaitForSeconds(SecondsToAnimate);
            }
            _chosenChip = null;
            _chipCanChange = true;
            CheckNeedToEat();
            _inMove = false;
        }

        public bool AreYouBusy()
        {
            return _inMove;
        }

        private void CheckNeedToEat()
        {
            _hungryChips.Clear();
            foreach (var chip in _allChips)
            {
                if (chip.IsWhite == _whiteMove)
                {
                    if (OnlyToEat(chip).Count > 0)
                    {
                        _hungryChips.Add(chip);
                        chip.Emit(ToEmit.Red);
                    }
                }
            }
        }

        public List<(Cell CellToMove, Chip ChipToEat)> OnlyToEat(Chip chip)
        {
            var allmoves = WhereCanMove(chip, true);
            return allmoves.Where(m => m.ChipToEat != null).ToList();
        }
        public List<(Cell CellToMove, Chip ChipToEat)> WhereCanMove(Chip chip, bool onlyEat = false)
        {
            Chip anotherchip = null;
            Chip eatenchip = null;
            List<(Cell CellToMove, Chip ChipToEat)> toreturn = new List<(Cell CellToMove, Chip ChipToEat)>();
            var moves = Enum.GetValues(typeof(Moves)).Cast<Moves>();
            foreach (var move in moves)
            {
                eatenchip = null;
                if (!chip.IsQueen)
                {
                    var nextpos = chip.Position.GetNext(move);
                    if (nextpos != null)
                    {
                        anotherchip = GetChip(nextpos);
                        if (anotherchip == null)
                        {
                            if (!onlyEat && _legitMoves[_whiteMove].Contains(move))
                            {
                                toreturn.Add((GetCell(nextpos), null));
                            }
                        }
                        else
                        {
                            if (anotherchip.IsWhite != chip.IsWhite)
                            {
                                var hoppos = nextpos.GetNext(move);
                                if (hoppos != null && GetChip(hoppos) == null)
                                {
                                    toreturn.Add((GetCell(hoppos), anotherchip));
                                }
                            }
                        }
                    }
                }
                else
                {
                    BaseCoord queenmove = chip.Position.GetNext(move);
                    while (queenmove != null)
                    {
                        anotherchip = GetChip(queenmove);
                        if (anotherchip == null)
                        {
                            toreturn.Add((GetCell(queenmove), eatenchip));
                        }
                        else
                        {
                            if (eatenchip != null) break;
                            if (anotherchip.IsWhite != chip.IsWhite)
                            {
                                eatenchip = anotherchip;
                            }
                            else
                            {
                                break;
                            }
                        }
                        queenmove = queenmove.GetNext(move);
                    }
                }
            }
            return toreturn;
        }

        private void ResetAllEmit()
        {
            _chosenChip.Emit(ToEmit.None);
            _moveList?.ForEach(c => c.CellToMove.Emit(ToEmit.None));
            _moveList?.Clear();
        }

        public bool IsHungry(Chip chip)
        {
            bool toreturn = false;
            var inhungry = _hungryChips.FirstOrDefault(h => h == chip);
            if (inhungry != null)
            {
                toreturn = true;
            }
            return toreturn;
        }

    }

    public interface ICanPlayCheckers
    {
        void ChooseChip(string chip);
        void MoveChip(string move);
        bool AreYouBusy();
    }
}

#region EditorCreationTry
//IEnumerator DestroyOldInEditor(GameObject go)
//{
//    yield return new WaitForSeconds(0);
//    DestroyImmediate(go);
//}

//private void OnValidate()
//{
//    return;
//    if (_CellPrefab != null && _CellBlack != null && _CellWhite != null)
//    {
//        CreateMap();
//    }
//    if (_ChipPrefab != null && _ChipBlack != null && _ChipWhite != null)
//    {
//        CreateChips();
//    }
//} 
#endregion
