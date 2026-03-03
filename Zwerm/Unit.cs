
namespace Zwerm
{
    class Unit
    {
        //Unit settings
        private double m_Schaalfactor;   //Conversie van afstand naar kracht op de Unit ;
        private int m_OptDist;      //Optimale afstand tussen twee units ;
        private int m_MaxMouseDist; //Afstand dat een unit van de mousepointer wegloopt ;
        private int m_NearNum;      //Aantal dichtbijzijnde units waarmee rekening gehouden wordt ;
        //Unit members
        private double m_VeldX;   //Veld breedte ;
        private double m_VeldY;   //Veld hoogte ;
        private double m_MouseX;  //X positie van de mouse pointer ;
        private double m_MouseY;  //Y positie van de mouse pointer ;
        private double m_X;       //X positie van de unit (0 .. Veld breedte) ;
        private double m_Y;       //Y positie van de unit (0 .. Veld hoogte) ;
        private double m_Speed;   //Snelheid van iedere Unit ;
        private double m_Dir;     //Richting waarin de Unit beweegt (0 .. 2*Pi radialen) ;
        private double[] m_NearestX;  //X positie van de nearest units (0 .. Veld breedte - 1) ;
        private double[] m_NearestY;  //Y positie van de nearest units (0 .. Veld hoogte - 1) ;

        //Constructors
        public Unit() //Unit met Default waarden
        {
            m_Schaalfactor = 0.001;
            m_OptDist = 8;
            m_MaxMouseDist = 40;
            m_NearNum = 20;
            m_NearestX = new double[20];
            m_NearestY = new double[20];
        }

        public Unit(double schaal, int Optdist, int MaxMouseDist, int NearNum)
        {
            m_Schaalfactor = schaal;
            m_OptDist = Optdist;
            m_MaxMouseDist = MaxMouseDist;
            m_NearNum = NearNum;
            m_NearestX = new double[NearNum];
            m_NearestY = new double[NearNum];
        }

        //Properties
        public double VeldX
        {
            get { return m_VeldX; }
            set { m_VeldX = value; }
        }

        public double VeldY
        {
            get { return m_VeldY; }
            set { m_VeldY = value; }
        }

        public double MouseX
        {
            get { return m_MouseX; }
            set { m_MouseX = value; }
        }

        public double MouseY
        {
            get { return m_MouseY; }
            set { m_MouseY = value; }
        }

        public double X
        {
            get { return m_X; }
            set { m_X = value; }
        }

        public double Y
        {
            get { return m_Y; }
            set { m_Y = value; }
        }

        public double Speed
        {
            get { return m_Speed; }
            set { m_Speed = value; }
        }

        public double Dir
        {
            get { return m_Dir; }
            set { m_Dir = value; }
        }

        public double GetNearestX(int Index)
        {
            return m_NearestX[Index];
        }

        public void SetNearestX(int Index, double value)
        {
            m_NearestX[Index] = value;
        }

        public double GetNearestY(int Index)
        {
            return m_NearestY[Index];
        }

        public void SetNearestY(int Index, double value)
        {
            m_NearestY[Index] = value;
        }

        public void Update()
        {
            double dist = 0.0;
            double FX = 0.0;
            double FY = 0.0;
            double VXN = 0.0;
            double VYN = 0.0;

            //Blijf bij de dichtsbijzijnde units (Zwerm gedrag!!).
            for (int I = 0; I < m_NearNum; I++)
            {
                if (m_NearestX[I] > 0 & m_NearestY[I] > 0)
                {
                    dist = Math.Sqrt((m_NearestX[I] - m_X) * (m_NearestX[I] - m_X) + (m_NearestY[I] - m_Y) * (m_NearestY[I] - m_Y));
                    if (dist > m_OptDist)
                    {
                        FX += (dist - m_OptDist) * (dist - m_OptDist) / dist * (m_NearestX[I] - m_X);
                        FY += (dist - m_OptDist) * (dist - m_OptDist) / dist * (m_NearestY[I] - m_Y);
                    }
                    else if (dist > 0)
                    {
                        FX -= 5 * (dist - m_OptDist) * (dist - m_OptDist) / dist * (m_NearestX[I] - m_X);
                        FY -= 5 * (dist - m_OptDist) * (dist - m_OptDist) / dist * (m_NearestY[I] - m_Y);
                    }
                }
            }
            //Keer terug naar scherm midden (Voorkomt het wegvliegen van de zwerm)
            dist = Math.Sqrt((m_VeldX / 2 - m_X) * (m_VeldX / 2 - m_X) + (m_VeldY / 2 - m_Y) * (m_VeldY / 2 - m_Y));
            if ((dist > (m_VeldX + m_VeldY) / 5))
            {
                FX += 4 * (m_VeldX / 2 - m_X);
                FY += 4 * (m_VeldX / 2 - m_Y);
            }
            //Vlucht weg van de mouse (externe actie)
            dist = Math.Sqrt((m_MouseX - m_X) * (m_MouseX - m_X) + (m_MouseY - m_Y) * (m_MouseY - m_Y));
            if ((dist < m_MaxMouseDist))
            {
                FX -= 500 * (m_MouseX - m_X);
                FY -= 500 * (m_MouseY - m_Y);
            }
            //Pas de bewegingsrichting aan
            VXN = m_Speed * Math.Cos(m_Dir) + m_Schaalfactor * FX;
            VYN = m_Speed * Math.Sin(m_Dir) + m_Schaalfactor * FY;
            m_Dir = Math.Atan2(VYN, VXN);
            //Update the Unit position
            m_X += m_Speed * Math.Cos(m_Dir);
            m_Y += m_Speed * Math.Sin(m_Dir);
            //De Unit moet binnen het Veld blijven
            if (m_X < 2) { m_X = 2; }
            if (m_Y < 2) { m_Y = 2; }
            if (m_X > m_VeldX - 2) { m_X = m_VeldX - 2; }
            if (m_Y > m_VeldY - 2) { m_Y = m_VeldY - 2; }
        }
    }
}
