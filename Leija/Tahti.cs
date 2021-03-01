using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Jypeli;
public class Tahti : PhysicsObject
{

    public Tahti(double leveys, double korkeus) : base(leveys, korkeus)
    {
        
    }

    public void TuhoaTahti()
    {
        this.Destroy();
    }

}

