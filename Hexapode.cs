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
        private double height = 300;

        //Plaque de base (0,0) au centre de la plaque
        public double rayVerBase; //Rayon du cercle des vérins
        public double alphaBase; //distance angulaire entre 2 vérins voisins
        public double betaBase;
        //Plateforme (0,0) au centre de la plaque
        public double rayVerPlat; //Rayon du cercle des vérins
        public double alphaPlat; //distance angulaire entre 2 vérins voisins
        public double betaPlat;

        Point3D centreRotation;

        //Degrees of freedom
        public double pitch = 0; //avant-arriere rotation (pitch, axe Y)
        public double roll = 0;  //gauche-droite rotation (roll, axe X)
        public double yaw = 0;   //tourner sur nous-même (yaw, axe Z)
        public double X = 0;     //avant-arriere déplacement (axe X, en avant positif)
        public double Y = 0;     //gauche-droite déplacement (axe Y, gauche positif)
        public double Z = 0;     //haut-bas déplacement (axe Z , haut positif)

        //liste de position des verrins
        Point3D[] posVerBase = new Point3D[6]; //TODO: ne pas oublier de modifier le point de bas
        Point3D[] posVerPlat = new Point3D[6];
        Matrix3D modificationMatrix; //matrice de modification final
        public double[] lengthVer = new double[6];  //longeur verrin
        //Calcul des angles des servos
        public double longBrasLev = 22;//longueur du bras de levier du servo
        public double longTringle = 174; //longueur des tringles entre la platform et la base 
        public double[] AngleServo = new double[6];

        private double speed = 15; //vitesse de déplacement du offset

        public void ResetPos()
        {
            X = 0;
            Y = 0;
            Z = 0;
            pitch = 0;
            roll = 0;
            yaw = 0;
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

            string data = $"{lengthVer[0]:0.000},{lengthVer[1]:0.000},{lengthVer[2]:0.000},{lengthVer[3]:0.000},{lengthVer[4]:0.000},{lengthVer[5]:0.000}";
            Debug.WriteLine(data);
            return data;
        }

        private void setDefault()//set la pose de base de l'exapode
        {
            pitch = 0;
            yaw = 0;
            roll = 0;
        }
    }
}
