

namespace Fluid_Sim
{
    ///Copyright 2022 Matthias Müller - Ten Minute Physics, 
    ///www.youtube.com/c/TenMinutePhysics
    ///www.matthiasMueller.info/tenMinutePhysics
    ///MIT License
    ///Converted to C# and Modified by Jos Gielen 2026
    public class Fluid
    {
        public double density;
        public int numX;
        public int numY;
        public double h;
        public double[,] u;
        public double[,] v;
        public double[,] newU;
        public double[,] newV;
        public double[,] p;
        public double[,] s;
        public double[,] m;
        public double[,] newM;

        public Fluid( double dens, int X, int Y, double hi )
        {
            density = dens;
            numX = X;
            numY = Y;
            h = hi;
            u = new double[numX, numY];
            v = new double[numX, numY];
            newU = new double[numX, numY];
            newV = new double[numX, numY];
            p = new double[numX, numY];
            s = new double[numX, numY];
            m = new double[numX, numY];
            newM = new double[numX, numY];
            for (int i = 0; i < numX; i++)
            {
                for (int j = 0; j < numY; j++)
                {
                    m[i, j] = 1.0;
                }
            }
        }

        public void simulate(double dt, double gravity, int numIters, double overRelaxation)
        {
            integrate(dt, gravity);
            for (int i = 0; i < numX; i++)
            {
                for (int j = 0; j < numY; j++)
                {
                    p[i, j] = 0.0;
                }
            }
            solveIncompressibility(numIters, dt, overRelaxation);
            extrapolate();
            advectVel(dt);
            advectSmoke(dt);
        }

        public void integrate(double dt, double gravity)
        {
            for (int i = 1; i < numX; i++)
            {
                for (int j = 1; j < numY - 1; j++)
                {
                    if (s[i , j] != 0.0 && s[i , j - 1] != 0.0)
                    {
                        v[i , j] += gravity * dt;
                    }
                }
            }
        }

        public void solveIncompressibility(int numIters, double dt, double overRelaxation)
        {
            int n = numY;
            double cp = density * h / dt;

            for (int iter = 0; iter < numIters; iter++)
            {
                for (int i = 1; i < numX - 1; i++)
                {
                    for (int j = 1; j < numY - 1; j++)
                    {
                        if (s[i , j] == 0.0) { continue; }
                        double sn = s[i , j];
                        double sx0 = s[i - 1 , j];
                        double sx1 = s[i + 1 , j];
                        double sy0 = s[i , j - 1];
                        double sy1 = s[i , j + 1];
                        sn = sx0 + sx1 + sy0 + sy1;
                        if (sn == 0.0) { continue; }
                        double div = u[i + 1 , j] - u[i , j] + v[i , j + 1] - v[i , j];
                        double p0 = -div / sn;
                        p0 *= overRelaxation;
                        p[i , j] += cp * p0;
                        u[i , j] -= sx0 * p0;
                        u[i + 1 , j] += sx1 * p0;
                        v[i , j] -= sy0 * p0;
                        v[i , j + 1] += sy1 * p0;
                    }
                }
            }
        }

        public void extrapolate()
        {
            for (int i = 0; i < numX; i++)
            {
                u[i , 0] = u[i , 1];
                u[i , numY - 1] = u[i , numY - 2];
            }
            for (int j = 0; j < numY; j++)
            {
                v[0 , j] = v[1 , j];
                v[numX - 1 , j] = v[numX - 2 , j];
            }
        }

        public double sampleField(double x, double y, int field)
        {
            double h1 = 1.0 / h;
            double h2 = 0.5 * h;
            double nx = Math.Max(Math.Min(x, numX * h), h);
            double ny = Math.Max(Math.Min(y, numY * h), h);
            double dx = 0.0;
            double dy = 0.0;
            double[,] f = new double[numX + 2,numY + 2];
            switch (field)
            {
                case 0: f = u; dy = h2; break;
                case 1: f = v; dx = h2; break;
                case 2: f = m; dx = h2; dy = h2; break;
            }
            int x0 = Math.Min((int)Math.Floor((nx - dx) * h1), numX - 1);
            double tx = ((nx - dx) - x0 * h) * h1;
            int x1 = Math.Min(x0 + 1, numX - 1);
            int y0 = Math.Min((int)Math.Floor((ny - dy) * h1), numY - 1);
            double ty = ((ny - dy) - y0 * h) * h1;
            int y1 = Math.Min(y0 + 1, numY - 1);
            double sx = 1.0 - tx;
            double sy = 1.0 - ty;
            double val = sx * sy * f[x0 , y0] + tx * sy * f[x1 , y0] + tx * ty * f[x1 , y1] + sx * ty * f[x0 , y1];
            return val;
        }

        public double avgU(int i, int j)
        {
            return (u[i , j - 1] + u[i , j] + u[i + 1 , j - 1] + u[i + 1 , j]) * 0.25;
        }

        public double avgV(int i, int j)
        {
            return (v[i - 1 , j] + v[i , j] + v[i - 1 , j + 1] + v[i , j + 1]) * 0.25;
        }

        public void advectVel(double dt)
        {
            for (int i = 0; i < numX; i++)
            {
                for (int j = 0; j < numY; j++)
                {
                    newU[i,j] = u[i,j];
                    newV[i,j] = v[i,j];
                }
            }
            double h2 = 0.5 * h;
            for (int i = 1; i < numX; i++)
            {
                for (int j = 1; j < numY; j++)
                {
                    // u component
                    if (s[i , j] != 0.0 && s[i - 1 , j] != 0.0 && j < numY - 1)
                    {
                        double x = i * h;
                        double y = j * h + h2;
                        double un = u[i , j];
                        double vn = avgV(i, j);
                        x = x - dt * un;
                        y = y - dt * vn;
                        un = sampleField(x, y, 0);
                        newU[i , j] = un;
                    }
                    // v component
                    if (s[i ,j] != 0.0 && s[i , j - 1] != 0.0 && i < numX - 1)
                    {
                        double x = i * h + h2;
                        double y = j * h;
                        double un = avgU(i, j);
                        double vn = v[i , j];
                        x = x - dt * un;
                        y = y - dt * vn;
                        vn = sampleField(x, y, 1);
                        newV[i , j] = vn;
                    }
                }
            }
            for (int i = 1; i < numX; i++)
            {
                for (int j = 1; j < numY; j++)
                {
                    u[i, j] = newU[i, j];
                    v[i, j] = newV[i, j];
                }
            }
        }

        public void advectSmoke(double dt)
        {
            for (int i = 0; i < numX; i++)
            {
                for (int j = 1; j < numY; j++)
                {
                    newM[i,j] = m[i,j];
                }
            }
            var h2 = 0.5 * h;
            for (int i = 1; i < numX - 1; i++)
            {
                for (int j = 1; j < numY - 1; j++)
                {
                    if (s[i , j] != 0.0)
                    {
                        double un = (u[i , j] + u[i + 1 , j]) * 0.5;
                        double vn = (v[i , j] + v[i , j + 1]) * 0.5;
                        double x = i * h + h2 - dt * un;
                        double y = j * h + h2 - dt * vn;
                        newM[i ,j] = sampleField(x, y, 2);
                    }
                }
            }
            for (int i = 0; i < numX; i++)
            {
                for (int j = 1; j < numY; j++)
                {
                    m[i, j] = newM[i, j];
                }
            }
        }
    }
}
