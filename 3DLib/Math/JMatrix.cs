using System;
using System.Collections.Generic;
using System.Text;

using System.Runtime.InteropServices;


namespace Library3d.Math
{
    
    public partial class JMatrix
    {
        public JMatrix()
        {
            Indetity();
        }

        public JMatrix(JMatrix mat)
        {
            Array.Copy(mat.f, this.f, mat.f.Length);
        }

        public void Indetity()
        {
            Zero();
            Diagonal(1.0f);
        }

        public void Diagonal(float d)
        {
            m11 = m22 = m33 = m44 = d;
        }


        public void Zero() { Array.Clear(f, 0, f.Length); }

        void Init(float m00, float m01, float m02, float m03,
                                             float m10, float m11, float m12, float m13,
                                             float m20, float m21, float m22, float m23,
                                             float m30, float m31, float m32, float m33)
        {
            m(0, 0, m00); m(0, 1, m01); m(0, 2, m02); m(0, 3, m03);
            m(1,0,m10); m(1,1, m11); m(1,2, m12); m(1,3, m13);
            m(2,0,m20); m(2,1, m21); m(2,2, m22); m(2,3, m23);
            m(3, 0, m30); m(3, 1, m31); m(3, 2, m32); m(3, 3, m33);


        }

        public  void	Multiply( JMatrix a, JMatrix b )
        {

           f[ 0] = b.f[ 0] * a.f[ 0] + b.f[ 4] * a.f[ 1] + b.f[ 8] * a.f[ 2];
           f[ 1] = b.f[ 1] * a.f[ 0] + b.f[ 5] * a.f[ 1] + b.f[ 9] * a.f[ 2];
           f[ 2] = b.f[ 2] * a.f[ 0] + b.f[ 6] * a.f[ 1] + b.f[10] * a.f[ 2];
           f[ 3] = 0;
           f[ 4] = b.f[ 0] * a.f[ 4] + b.f[ 4] * a.f[ 5] + b.f[ 8] * a.f[ 6];
           f[ 5] = b.f[ 1] * a.f[ 4] + b.f[ 5] * a.f[ 5] + b.f[ 9] * a.f[ 6];
           f[ 6] = b.f[ 2] * a.f[ 4] + b.f[ 6] * a.f[ 5] + b.f[10] * a.f[ 6];
           f[ 7] = 0;
           f[ 8] = b.f[ 0] * a.f[ 8] + b.f[ 4] * a.f[ 9] + b.f[ 8] * a.f[10];
           f[ 9] = b.f[ 1] * a.f[ 8] + b.f[ 5] * a.f[ 9] + b.f[ 9] * a.f[10];
           f[10] = b.f[ 2] * a.f[ 8] + b.f[ 6] * a.f[ 9] + b.f[10] * a.f[10];
           f[11] = 0;
           f[12] = b.f[ 0] * a.f[12] + b.f[ 4] * a.f[13] + b.f[ 8] * a.f[14] + b.f[12];
           f[13] = b.f[ 1] * a.f[12] + b.f[ 5] * a.f[13] + b.f[ 9] * a.f[14] + b.f[13];
           f[14] = b.f[ 2] * a.f[12] + b.f[ 6] * a.f[13] + b.f[10] * a.f[14] + b.f[14];
           f[15] = 1;
        }

        public  void	MultiplyInv( JMatrix a, JMatrix b )
        {

           JVector ip=b.VectorTransformInvNoPos( b.Pos );
           //
           f[ 0] = b.f[ 0] * a.f[ 0] + b.f[ 1] * a.f[ 1] + b.f[ 2] * a.f[ 2];
           f[ 1] = b.f[ 4] * a.f[ 0] + b.f[ 5] * a.f[ 1] + b.f[ 6] * a.f[ 2];
           f[ 2] = b.f[ 8] * a.f[ 0] + b.f[ 9] * a.f[ 1] + b.f[10] * a.f[ 2];
           f[ 3] = 0;
           f[ 4] = b.f[ 0] * a.f[ 4] + b.f[ 1] * a.f[ 5] + b.f[ 2] * a.f[ 6];
           f[ 5] = b.f[ 4] * a.f[ 4] + b.f[ 5] * a.f[ 5] + b.f[ 6] * a.f[ 6];
           f[ 6] = b.f[ 8] * a.f[ 4] + b.f[ 9] * a.f[ 5] + b.f[10] * a.f[ 6];
           f[ 7] = 0;
           f[ 8] = b.f[ 0] * a.f[ 8] + b.f[ 1] * a.f[ 9] + b.f[ 2] * a.f[10];
           f[ 9] = b.f[ 4] * a.f[ 8] + b.f[ 5] * a.f[ 9] + b.f[ 6] * a.f[10];
           f[10] = b.f[ 8] * a.f[ 8] + b.f[ 9] * a.f[ 9] + b.f[10] * a.f[10];
           f[11] = 0;
           //
           f[12] = b.f[ 0] * a.f[12] + b.f[ 1] * a.f[13] + b.f[ 2] * a.f[14] - ip.x;
           f[13] = b.f[ 4] * a.f[12] + b.f[ 5] * a.f[13] + b.f[ 6] * a.f[14] - ip.y;
           f[14] = b.f[ 8] * a.f[12] + b.f[ 9] * a.f[13] + b.f[10] * a.f[14] - ip.z;
           //
           f[15] = 1;
           /**/
        }

        public JVector VectorTransformInvNoPos(JVector v ) 
        {
	        return new JVector(	v.x*m(0,0) + v.y*m(0,1) + v.z*m(0,2),
							        v.x*m(1,0) + v.y*m(1,1) + v.z*m(1,2),
                                    v.x * m(2, 0) + v.y * m(2, 1) + v.z * m(2, 2), 
							        1 );
        } 

        public  JMatrix Transpose( JMatrix mat )
        {
	        // Transpose matrix only (not position!)
           m(0,0,mat.m(0,0));
           m(0,1,mat.m(1,0));
           m(0,2,mat.m(2,0));
           m(0,3,0);
           m(1,0,mat.m(0,1));
           m(1,1,mat.m(1,1));
           m(1,2,mat.m(2,1));
           m(1,3,0);
           m(2,0,mat.m(0,2));
           m(2,1,mat.m(1,2));
           m(2,2,mat.m(2,2));
           m(2,3,0);
           m(3,0,0);
           m(3,1,0);
           m(3,2,0);
           m(3, 3, 1);
	        //
	       return this;
        }

        public void Set(JMatrix mat)
        {
            Array.Copy(mat.f, this.f, mat.f.Length);
        }

        public JMatrix Inverse( JMatrix m1)
        {
            JMatrix m = new JMatrix(this);
            m=Transpose(m1);
            Set(m);

#warning TODO
            JVector res = (m1.Pos * this).Neg();
	        return this;
        }

        public  JMatrix Inverse2( JMatrix a)
        {
           JVector opos=a.Pos;
           //
           f[ 0]=a.f[ 0];
           f[ 1]=a.f[ 4];
           f[ 2]=a.f[ 8];
           f[ 3]=0;
           f[ 4]=a.f[ 1];
           f[ 5]=a.f[ 5];
           f[ 6]=a.f[ 9];
           f[ 7]=0;
           f[ 8]=a.f[ 2];
           f[ 9]=a.f[ 6];
           f[10]=a.f[10];
           f[11]=0;
           f[12]=0;
           f[13]=0;
           f[14]=0;
           //
           Pos=(opos * this).Neg();
           //
           f[15]=1;
           //
           return this;
        }




        //
        public JVector VectorTransform( JVector v ) 
        {
	        return new JVector(	v.x*m(0,0) + v.y*m(1,0) + v.z*m(2,0) + m(3,0),
                                    v.x * m(0, 1) + v.y * m(1, 1) + v.z * m(2, 1) + m(3, 1),
							        v.x*m(0,2) + v.y*m(1,2) + v.z*m(2,2) + m(3,2), 
							        1 );
        }

        public JVector VectorTransformNoPos(  JVector v ) 
        {
            return new JVector(v.x * m(0, 0) + v.y * m(1, 0) + v.z * m(2, 0),
							        v.x*m(0,1) + v.y*m(1,1) + v.z*m(2,1),
                                    v.x * m(0, 2) + v.y * m(1, 2) + v.z * m(2, 2), 
							        1 );
        }

        public JVector VectorTransform4x4(  JVector v ) 
        {
	        return new JVector(	v.x*m(0,0) + v.y*m(1,0) + v.z*m(2,0) + m(3,0),
							        v.x*m(0,1) + v.y*m(1,1) + v.z*m(2,1) + m(3,1),
							        v.x*m(0,2) + v.y*m(1,2) + v.z*m(2,2) + m(3,2), 
							        v.x*m(0,3) + v.y*m(1,3) + v.z*m(2,3) + m(3,3) );
        }

//

        public JVector VectorTransformInv( JVector v ) 
        {
           JVector p=VectorTransformInvNoPos( Pos );
           //
           return new JVector(v.x * m(0, 0) + v.y * m(0, 1) + v.z * m(0, 2),
                                    v.x * m(1, 0) + v.y * m(1, 1) + v.z * m(1, 2),
							        v.x*m(2,0) + v.y*m(2,1) + v.z*m(2,2), 
							        1 ) - p;
        }



        public JVector GetCol( int col ) 
        {
           return new JVector( m(0,col) , m(1,col) , m(2,col) , m(3,col)); 
        }

        public void SetCol( int col, JVector colv )
        {
           m(0,col,colv.x); 
           m(1,col,colv.y); 
           m(2,col,colv.z); 
           m(3,col,colv.w); 
        }

        public JVector GetRow( int row ) 
        {
            return v(row);
        }
    
        public void SetRow( int row, JVector rowv )	
        {
            v(row,rowv);
        }

        public  static JMatrix operator*(JMatrix oldmat, float a) 
        {
	       JMatrix newm = new JMatrix();
           newm.v(0, oldmat.v(0) * a);
           newm.v(1, oldmat.v(1) * a);
           newm.v(2, oldmat.v(2) * a);
           newm.v(3, oldmat.v(3) * a);
           newm.m(0, 3, oldmat.m(0, 3) * a);
           newm.m(1, 3, oldmat.m(1, 3) * a);
           newm.m(2, 3, oldmat.m(2, 3) * a);
           newm.m(3, 3, oldmat.m(3, 3) * a);
	       return newm;
        }

        public static JMatrix                             operator*( JMatrix t,JMatrix m ) 
       {
	       JMatrix  newm = new JMatrix();
	       newm.Multiply( t, m );
	       return   newm;
       }

        
        public static JMatrix                             operator/( JMatrix t,JMatrix m )
        {
            JMatrix newm = new JMatrix();
	        newm.MultiplyInv( t, m );
	        return   newm;
        }
        public void Scale(float d)
        {
            m11 *= d; m22 *= d; m33 *= d; m44 *= 1;
        }

        public static JVector operator*( JVector v , JMatrix m )
        {
	        return m.VectorTransform( v );
        }

        public static JVector operator/( JVector v , JMatrix m )
        {
	        return m.VectorTransformInv( v );
        }


    }
}
