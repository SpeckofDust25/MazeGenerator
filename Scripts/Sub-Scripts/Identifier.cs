using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;


public class Identifier
{
    public int uid;
    public bool used;

    public Identifier(int _uid)
    {
        uid = _uid;
        used = false;
    }
}
