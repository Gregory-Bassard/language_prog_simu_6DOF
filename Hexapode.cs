using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO.Ports;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Media.Media3D;

namespace language_prog_simu_6DOF
{
    class Hexapode
    {

        public double height;

        //Plaque de base (0,0) au centre de la plaque
        public double rayVerBase; //Rayon du cercle des vérins
        public double alphaBase; //distance angulaire entre 2 vérins voisins
        public double betaBase;
        //Plateforme (0,0) au centre de la plaque
        public double rayVerPlat; //Rayon du cercle des vérins
        public double alphaPlat; //distance angulaire entre 2 vérins voisins
        public double betaPlat;

        public Point3D centreRotation;

        //Degrees of freedom
        public double pitch = 0; //avant-arriere rotation (pitch, axe Y)
        public double roll = 0;  //gauche-droite rotation (roll, axe X)
        public double yaw = 0;   //tourner sur nous-même (yaw, axe Z)
        public double X = 0;     //avant-arriere déplacement (axe X, en avant positif)
        public double Y = 0;     //gauche-droite déplacement (axe Y, gauche positif)
        public double Z = 0;     //haut-bas déplacement (axe Z , haut positif)

        private double resetX = 0;
        private double resetY = 0;
        private double resetZ = 0;
        private double resetYaw = 0;
        private double resetPitch = 0;
        private double resetRoll = 0;

        //liste de position des verrins
        Point3D[] posVerBase = new Point3D[6]; //TODO: ne pas oublier de modifier le point de bas
        Point3D[] posVerPlat = new Point3D[6];
        Matrix3D modificationMatrix; //matrice de modification final
        public double[] lengthVer = new double[6];  //longeur verrin

        public Hexapode(double x, double y, double z, double yaw, double pitch, double roll)
        {
            this.resetX = this.X = x;
            this.resetY = this.Y = y;
            this.resetZ = this.Z = z;
            this.resetYaw = this.yaw = yaw;
            this.resetPitch = this.pitch = pitch;
            this.resetRoll = this.roll = roll;
        }

        public Hexapode(double[] pos)   
        {
            this.resetX = this.X = pos[0];
            this.resetY = this.Y = pos[1];
            this.resetZ = this.Z = pos[2];
            this.resetYaw = this.yaw = pos[3];
            this.resetPitch = this.pitch = pos[4];
            this.resetRoll = this.roll = pos[5];
        }

        public void ResetPos()
        {
            X = resetX;
            Y = resetY;
            Z = resetZ;
            yaw = resetYaw;
            pitch = resetPitch;
            roll = resetRoll;
        }
        public void Update()
        {
            //CalculPosHexapode();
            //calcul des point du simulateur (X,Y,Z)
            posVerBase[0] = new Point3D(-(-rayVerBase) * Math.Sin(-(alphaBase / 2 + betaBase)), -rayVerBase * Math.Cos(-(alphaBase / 2 + betaBase)), 0);//verrin 1
            posVerBase[1] = new Point3D(-(-rayVerBase) * Math.Sin(-alphaBase / 2), -rayVerBase * Math.Cos(-alphaBase / 2), 0);//verrin 2
            posVerBase[2] = new Point3D(-(-rayVerBase) * Math.Sin(alphaBase / 2), -rayVerBase * Math.Cos(alphaBase / 2), 0);//verrin 3
            posVerBase[3] = new Point3D(-(-rayVerBase) * Math.Sin(alphaBase / 2 + betaBase), -rayVerBase * Math.Cos(alphaBase / 2 + betaBase), 0);//verrin 4
            posVerBase[4] = new Point3D(-(-rayVerBase) * Math.Sin(alphaBase * 1.5 + betaBase), -rayVerBase * Math.Cos(alphaBase * 1.5 + betaBase), 0);//verrin 5
            posVerBase[5] = new Point3D(-(-rayVerBase) * Math.Sin(-(alphaBase * 1.5 + betaBase)), -rayVerBase * Math.Cos(-(alphaBase * 1.5 + betaBase)), 0);//verrin 6

            posVerPlat[0] = new Point3D(-rayVerPlat * Math.Sin(alphaPlat / 2 + betaPlat), rayVerPlat * Math.Cos(alphaPlat / 2 + betaPlat), height);//verrin 1
            posVerPlat[1] = new Point3D(-rayVerPlat * Math.Sin(alphaPlat * 1.5 + betaPlat), rayVerPlat * Math.Cos(alphaPlat * 1.5 + betaPlat), height);//verrin 2
            posVerPlat[2] = new Point3D(-rayVerPlat * Math.Sin(-(alphaPlat * 1.5 + betaPlat)), rayVerPlat * Math.Cos(-(alphaPlat * 1.5 + betaPlat)), height);//verrin 3
            posVerPlat[3] = new Point3D(-rayVerPlat * Math.Sin(-(alphaPlat / 2 + betaPlat)), rayVerPlat * Math.Cos(-(alphaPlat / 2 + betaPlat)), height);//verrin 4
            posVerPlat[4] = new Point3D(-rayVerPlat * Math.Sin(-alphaPlat / 2), rayVerPlat * Math.Cos(-alphaPlat / 2), height);//verrin 5
            posVerPlat[5] = new Point3D(-rayVerPlat * Math.Sin(alphaPlat / 2), rayVerPlat * Math.Cos(alphaPlat / 2), height);//verrin 6
        }

        public void CalculPosHexapode()
        { //Calcule les angle de l'hexapode en fonction des valeur (pitch,yaw,roll,X,Y,Z)

            Vector3D offset = new Vector3D(X, Y, Z);//déplacement en x,y,z
            modificationMatrix = Calculation.GetModificationMatrix(yaw, pitch, roll, centreRotation, offset); //matrice de modification qui contient les mouvements(déplacment et rotation)

            for (int i = 0; i < 6; i++)
            {
                posVerPlat[i] = Point3D.Multiply(posVerPlat[i], modificationMatrix); //ajout de la matrice de modification

                lengthVer[i] = Math.Sqrt(Math.Pow(posVerPlat[i].X - posVerBase[i].X, 2) + Math.Pow(posVerPlat[i].Y - posVerBase[i].Y, 2) + Math.Pow(posVerPlat[i].Z - posVerBase[i].Z, 2));
            }
            centreRotation.X += X - centreRotation.X;
            centreRotation.Y += Y - centreRotation.Y;
            centreRotation.Z += Z + height - centreRotation.Z;
        }
        public string GetData()
        {
            for (int i = 0; i < 6; i++)
            {
                lengthVer[i] = (lengthVer[i] / 10 - 28.5) / 20 * 3.3;
                lengthVer[i] = lengthVer[i] > 3.3 ? 3.3 : lengthVer[i];
                lengthVer[i] = lengthVer[i] < 0 ? 0 : lengthVer[i];
            }
            return $"{lengthVer[0]:0.000},{lengthVer[1]:0.000},{lengthVer[2]:0.000},{lengthVer[3]:0.000},{lengthVer[4]:0.000},{lengthVer[5]:0.000}";
        }
        public double[] GetPos()
        {
            double[] pos = new double[6];
            pos[0] = X;
            pos[1] = Y;
            pos[2] = Z;
            pos[3] = yaw;
            pos[4] = pitch;
            pos[5] = roll;

            return pos;
        }
    }
}
