
namespace Zwerm
{
    class Swarm
    {
        //Zwerm Settings
        private double m_Schaalfactor;   //Conversie van afstand naar kracht op de Unit ;
        private int m_OptDist;      //Optimale afstand tussen twee units ;
        private int m_MaxMouseDist; //Afstand dat een unit van de mousepointer wegloopt ;
        private int m_NumNear;      //Aantal dichtbijzijnde units waarmee rekening gehouden wordt ;
        private int m_Aantal;       //Totaal aantal Units in de zwerm ;
        private Unit[] m_Units;
        private int m_MaxViewdist;
        private static Random Rnd = new Random();

        public Swarm() //Default zwerm met 100 units.
        {
            m_Units = new Unit[100];
            m_Aantal = 100;
            m_NumNear = 10;
            m_Schaalfactor = 0.0001;
            m_OptDist = 8;
            m_MaxMouseDist = 40;
            m_MaxViewdist = 20;
        }

        public Swarm(int Count)
        {
            m_Units = new Unit[Count];
            m_Aantal = Count;
            m_NumNear = 10;
            m_Schaalfactor = 0.0001;
            m_OptDist = 8;
            m_MaxMouseDist = 40;
            m_MaxViewdist = 20;
        }

        public double Scale
        {
            get { return m_Schaalfactor; }
            set { m_Schaalfactor = value; }
        }

        public int Optdistance
        {
            get { return m_OptDist; }
            set { m_OptDist = value; }
        }

        public int MaxMouseDistance
        {
            get { return m_MaxMouseDist; }
            set { m_MaxMouseDist = value; }
        }

        public int MaxViewDistance
        {
            get { return m_MaxViewdist; }
            set { m_MaxViewdist = value; }
        }

        public int NumNear
        {
            get { return m_NumNear; }
            set { m_NumNear = value; }
        }

        public int Aantal
        {
            get { return m_Aantal; }
            set { m_Aantal = value; }
        }

        public void SetMouse(double x, double y)
        {
            for (int I = 0; I < m_Aantal; I++)
            {
                m_Units[I].MouseX = x;
                m_Units[I].MouseY = y;
            }
        }

        public double getUnitX(int index)
        {
            return m_Units[index].X;
        }

        public double getUnitY(int index)
        {
            return m_Units[index].Y;
        }

        public void InitUnits(int h, int b, double s)
        {
            double X = 0.0;
            double Y = 0.0;
            double dir = 0.0;

            for (int I = 0; I < m_Aantal; I++)
            {
                m_Units[I] = new Unit(m_Schaalfactor, m_OptDist, m_MaxMouseDist, m_NumNear);
                //X = b / 3 * (1 + Rnd.NextDouble());
                //Y = h / 3 * (1 + Rnd.NextDouble());
                X = b * Rnd.NextDouble();
                Y = h * Rnd.NextDouble();
                dir = 2 * 3.1415926 * Rnd.NextDouble();
                m_Units[I].VeldX = b - 1;
                m_Units[I].VeldY = h - 1;
                m_Units[I].X = X;
                m_Units[I].Y = Y;
                m_Units[I].Speed = s;
                m_Units[I].Dir = dir;
            }
        }

        public void SetNearestUnits(int index)
        {
            double[] NearX = new double[m_NumNear];
            double[] NearY = new double[m_NumNear];
            double[] Nearest = new double[m_NumNear];
            double Dist = 0.0;
            double MaxNearest = 0.0;  //Grootste waarde in Nearest() ;
            int MaxNearestIndex = 0;      //Index van de grootste waarde in Nearest() ;
            double X0 = 0.0;
            double Y0 = 0.0;
            double X1 = 0.0;
            double Y1 = 0.0;

            for (int I = 0; I < m_NumNear; I++)
            {
                Nearest[I] = 1000000;
            }
            X0 = m_Units[index].X;
            Y0 = m_Units[index].Y;
            for (int J = 0; J < m_Aantal; J++)
            {
                if (J != index)
                {
                    X1 = m_Units[J].X;
                    Y1 = m_Units[J].Y;
                    Dist = Math.Sqrt((X1 - X0) * (X1 - X0) + (Y1 - Y0) * (Y1 - Y0));
                    if (Dist < m_MaxViewdist)
                    {
                        //Zoek de grootste waarde in Nearest()
                        MaxNearest = Nearest[0];
                        MaxNearestIndex = 0;
                        for (int K = 0; K < m_NumNear; K++)
                        {
                            if (Nearest[K] > MaxNearest)
                            {
                                MaxNearest = Nearest[K];
                                MaxNearestIndex = K;
                            }
                        }
                        if (Dist < MaxNearest)
                        {
                            Nearest[MaxNearestIndex] = Dist;
                            NearX[MaxNearestIndex] = X1;
                            NearY[MaxNearestIndex] = Y1;
                        }
                    }
                }
            }
            for (int J = 0; J < m_NumNear; J++)
            {
                m_Units[index].SetNearestX(J, NearX[J]);
                m_Units[index].SetNearestY(J, NearY[J]);
            }
        }

        public void UpdateUnits()
        {
            for (int I = 0; I < m_Aantal; I++)
            {
                SetNearestUnits(I);
            }
            for (int I = 0; I < m_Aantal; I++)
            {
                m_Units[I].Update();
            }
        }

    }
}
