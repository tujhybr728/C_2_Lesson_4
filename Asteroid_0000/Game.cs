using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Drawing;
using System.Windows.Forms;

namespace Asteroid_0000
{
    class Game
    {
        //здесь всё что используется в данном классе
        #region Value
        private static BufferedGraphicsContext _context;
        public static BufferedGraphics Buffer;
        public static int Width { get; set; }
        public static int Height { get; set; }
        private static Random rnd = new Random(); // нужна для рандомного появления объектов и нестатичности первого объекта
        private static Image background = Image.FromFile(@"Stars.jpg"); // задний фон
        private static Timer timer = new Timer { Interval = 10 }; // интервал уменьшен для плавности [но с игроком не работает :( ]
        public static List<Enemy> _listaster = new List<Enemy>(); //лист для астероидов
        public static List<Healing> _listhealing = new List<Healing>(); //лист для хилок
        public static List<Bullet> _listbullet = new List<Bullet>(); // лист пуль
        public static Enemy[] _aster;    // массив маленьких объектов (взаимодействие) (костыль)
        public static BigObj[] _bigobjs; // генерация массива объектов (круги)
        public static Player _player = new Player(new Point(100, 100), new Point(0, 0), new Size(20, 20)); // игрок
        public static Healing[] _healing; // хилки (костыль)
        private static int Record, RemoverA, RemoverH, RemoverB, ChekA, ChekH, ChekB, levelconst; // переменные записи и счёткики
        private static int levelchek = -1;

        #endregion

        // Здесь находиться всё что связано с иницииализацией
        #region InitInfo

        private static void Timer_Tick(object sender, EventArgs e)
        {
            if (levelchek < levelconst) Load();
            
            Draw();
            Update();            
        }



        static Game()
        {

        }



        public static void Init(Form form)
        {
            
                        
            Graphics g;         // Графическое устройство для вывода графики
            
            _context = BufferedGraphicsManager.Current;     // Предоставляет доступ к главному буферу графического контекста для текущего приложения
            g = form.CreateGraphics();      // Создаем объект (поверхность рисования) и связываем его с формой                                         
            Width = form.ClientSize.Width;   // Запоминаем размеры формы
            Height = form.ClientSize.Height; 
            Buffer = _context.Allocate(g, new Rectangle(0, 0, Width, Height));// Связываем буфер в памяти с графическим объектом, чтобы рисовать в буфере
            form.KeyDown += Form_KeyDown;
            timer.Tick += Timer_Tick;
            Load();            // загрузка объектов
            timer.Start(); //таймер задержки            
            Player.MessageDie += Finish;
        }

        #endregion
        //здесь находиться всё что связано с загрузкой и инициализацией данных
        #region Load


        
        public static void Load()
        {
            _listbullet.Clear();
            _listaster.Clear();
            _listhealing.Clear();
            levelchek = levelconst;         
            int rndsize;
            _aster = new Enemy[values.maxobjenemy+levelconst];
            _bigobjs = new BigObj[levelconst];
            //_bullet = new Bullet(new Point(0, 0), new Point(0, 0), new Size(0, 0));
            _healing = new Healing[levelconst/3];
            for (int i = 0; i < _aster.Length; i++)
            {
                //rndsize = rnd.Next(values.minsize, values.maxxize); // нужна для разнообразия размера объектов
                _listaster.Add(new Enemy(new Point(rnd.Next(100, 500), rnd.Next(100, 500)), new Point(rnd.Next(values.maxspeed),
                    rnd.Next(values.maxspeed)), new Size(20, 20))); // добавляем астероиды в лист-астероиды
            }
                        
            for (int i = 0; i < _bigobjs.Length; i++)
            {
                 rndsize = rnd.Next(values.sizebigobj/2,values.sizebigobj); // нужна для разнообразия размера объектов
                _bigobjs[i] = new BigObj(new Point(rnd.Next(0, 500), rnd.Next(0, 500)), new Point(5+rnd.Next(values.maxspeed),
                    rnd.Next(values.maxspeed)), new Size(rndsize, rndsize));
            }

            for (int i = 0; i < _healing.Length; i++)
            {
                _listhealing.Add (new Healing(new Point(rnd.Next(100, 500), rnd.Next(100, 500)), new Point(4 + rnd.Next(values.maxspeed),
                     rnd.Next(values.maxspeed)), new Size(20, 20)));
            }

        }

        #endregion

        // Здесь находиться всё что требует обновления
        #region UbdateInfo 

        public static void Update() // метод обновления
        {
            ChekA = 0;
            RemoverA = -1;
            RemoverH = -1;            
            foreach (Bullet bul in _listbullet) // обновляет пули
            {
                bul.Update();                
            }
            if (_listbullet == null)
            {
                Bullet sten = _listbullet.ElementAt(0); // переменная для обнаружения пуль за пределами карты
                if (sten.Pos.X > Game.Width + 50) _listbullet.RemoveAt(0); // удаляет пули за пределами карты
            }
            
            foreach (BigObj obj in _bigobjs) // обновление положения больших объектов
                obj.Update();

            ChekH = 0;
            foreach (Healing h in _listhealing) // обновление положения хилящих сфер
            {
                h.Update();
                if (_player.Collision(h) && _player.Energy < 100)
                {
                    _player.EnergyLow(-values.power);
                    RemoverH = ChekA;
                }
                ChekH++;
            }


            foreach (Enemy aster in _listaster) // обновление положения астероидов
            {
                aster.Update();
                ChekB = 0;
                RemoverB = -1;
                foreach (Bullet bul in _listbullet)
                {
                    if (aster.Collision(bul)) // астероид сбила пуля
                    {
                        System.Media.SystemSounds.Hand.Play();
                        RemoverA = ChekA; //запоминается астероид в который попала пуля
                        RemoverB = ChekB; //запоминается пуля в которую попал астероид
                        Record = Record + 100;
                    }
                    ChekB++;
                }
                if (RemoverB >= 0) _listbullet.RemoveAt(RemoverB);
                if (_player.Collision(aster))
                {
                    _player.EnergyLow(values.power);
                    System.Media.SystemSounds.Beep.Play();
                    RemoverA = ChekA;// запоминается астероид который попал в игрока
                }
                ChekA++;
            }

            if (RemoverA >= 0) _listaster.RemoveAt(RemoverA);

            if (RemoverH >= 0) _listhealing.RemoveAt(RemoverH);

            if (_player.Energy == 0)
            {
                Buffer.Graphics.DrawImage(background, new Point(-(Width / 2), -(Height / 2)));
                _player.Die();
            }

            _player.Update();

            if (_listaster.Count == 0) levelconst = levelconst + 1;
        }

        public static void Draw() //отоброжение объектов
        {
            Buffer.Graphics.DrawImage(background, new Point(-(Width / 2), -(Height / 2)));
            foreach (BigObj obj in _bigobjs) // сначала большие объекты
                obj.Draw();
            foreach (Enemy a in _listaster) // затем маленькие
                a.Draw();
            foreach (Healing h in _listhealing) // затем маленькие
                h.Draw();
            foreach (Bullet bul in _listbullet) // сначала большие объекты
                bul.Draw();
            _player.Draw();
            if (_player != null)
                Buffer.Graphics.DrawString("Energy:" + _player.Energy, SystemFonts.DefaultFont, Brushes.White, 0, 0);
            Buffer.Graphics.DrawString("Record:  " + Record, SystemFonts.DefaultFont, Brushes.White, 100, 0);
            Buffer.Graphics.DrawString("Level:  " + levelconst, SystemFonts.DefaultFont, Brushes.White, 300, 0);
            // счётчик попаданий по астероидам
            Buffer.Render();
        }

        #endregion

        // Здесь находиться всё что связано с игроком
        #region Player
        public static void Form_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.KeyCode == Keys.ControlKey) _listbullet.Add(new Bullet(new Point(_player.Rect.X + 10, _player.Rect.Y + 4), new Point(9, 0), new Size(4, 4)));
            if (e.KeyCode == Keys.W) _player.Up(10);
            if (e.KeyCode == Keys.S) _player.Down(10);
            if (e.KeyCode == Keys.A) _player.Left(10);
            if (e.KeyCode == Keys.D) _player.Rigth(10);
        }

        

        public static void Finish()
        {
            timer.Stop();
            Buffer.Graphics.DrawString("The End", new Font(FontFamily.GenericSansSerif, 60, FontStyle.Underline), Brushes.White, 200, 100);
            Buffer.Graphics.DrawString("Record: " + Record, new Font(FontFamily.GenericSansSerif, 60, FontStyle.Underline), Brushes.White, 200, 200);
            Buffer.Render();
        }
        #endregion

    }
}