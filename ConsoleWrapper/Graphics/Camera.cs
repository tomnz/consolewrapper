using System;
using System.Collections.Generic;
using System.Text;
using Microsoft.DirectX;
using Microsoft.DirectX.Direct3D;

namespace ConsoleWrapper
{
    class Camera
    {
        private Vector3 _location;
        private Vector3 _velLoc = new Vector3(0, 0, 0);

        private Vector3 _lookAt;
        private Vector3 _velLook = new Vector3(0, 0, 0);

        private float _aspectRatio = 1.0f;
        private int _width;
        private int _height;
        public float AspectRatio
        {
            get { return _aspectRatio; }
            set { _aspectRatio = value; }
        }

        private Vector3 _targetLocation;
        public Vector3 TargetLocation
        {
            get { return _targetLocation; }
            set { _targetLocation = value; }
        }
        private Vector3 _targetLookAt;
        public Vector3 TargetLookAt
        {
            get { return _targetLookAt; }
            set { _targetLookAt = value; }
        }

        private Vector3 _lightLocation;
        private Vector3 _velLightLoc = new Vector3(0, 0, 0);

        private Vector3 _lightLookAt;
        private Vector3 _velLightLook = new Vector3(0, 0, 0);

        private float _accelFactorLocation = 2.0f;
        private float _velFactorLocation = 0.4f;

        private float _accelFactorLook = 4.0f;
        private float _velFactorLook = 0.5f;

        private float _accelFactorLightLocation = 2.0f;
        private float _velFactorLightLocation = 0.4f;

        private float _accelFactorLightLook = 4.0f;
        private float _velFactorLightLook = 0.4f;

        private Matrix _viewMatrix;
        private Matrix _projectionMatrix;

        public Camera(Vector3 location, Vector3 lookAt)
        {
            _location = location;
            _lightLocation = location;
            _targetLocation = location;

            _lookAt = lookAt;
            _lightLookAt = lookAt;
            _targetLookAt = lookAt;
        }

        public Camera(Vector3 location, Vector3 lookAt, float aspectRatio, int width, int height)
        {
            _location = location;
            _lightLocation = location;
            _targetLocation = location;

            _lookAt = lookAt;
            _lightLookAt = lookAt;
            _targetLookAt = lookAt;

            _aspectRatio = aspectRatio;
            _width = width;
            _height = height;
        }

        public void Animate(float time, Light light)
        {
            // Update acceleration
            Vector3 accelLoc = Vector3.Scale(Vector3.Subtract(_targetLocation, _location), _accelFactorLocation);
            Vector3 accelLook = Vector3.Scale(Vector3.Subtract(_targetLookAt, _lookAt), _accelFactorLook);
            Vector3 accelLightLoc = Vector3.Scale(Vector3.Subtract(_targetLocation, _lightLocation), _accelFactorLightLocation);
            Vector3 accelLightLook = Vector3.Scale(Vector3.Subtract(_targetLookAt, _lightLookAt), _accelFactorLightLook);
            accelLoc.Scale(time);
            accelLook.Scale(time);
            accelLightLoc.Scale(time);
            accelLightLook.Scale(time);

            // Update velocity
            _velLoc.Add(accelLoc);
            _velLoc.Scale(_velFactorLocation);
            _velLook.Add(accelLook);
            _velLook.Scale(_velFactorLook);
            _velLightLoc.Add(accelLightLoc);
            _velLightLoc.Scale(_velFactorLightLocation);
            _velLightLook.Add(accelLightLook);
            _velLightLook.Scale(_velFactorLightLook);
            
            // Update position
            Vector3 tempVel = _velLoc;
            tempVel.Scale(time);
            _location.Add(_velLoc);
            tempVel = _velLook;
            tempVel.Scale(time);
            _lookAt.Add(_velLook);
            tempVel = _velLightLoc;
            tempVel.Scale(time);
            _lightLocation.Add(_velLightLoc);
            tempVel = _velLightLook;
            tempVel.Scale(time);
            _lightLookAt.Add(_velLightLook);

            light.Position = _lightLocation;
            light.Direction = Vector3.Subtract(_lightLookAt, _lightLocation);
            light.Enabled = true;
        }

        public void MoveCamera(Vector3 diff)
        {
            _location.Add(diff);
            _lookAt.Add(diff);
            _targetLocation.Add(diff);
            _targetLookAt.Add(diff);
        }

        public void SetupMatrices(Device device)
        {
            // Set up our view matrix. A view matrix can be defined given an eye point,
            // a point to lookat, and a direction for which way is up. Here, we set the
            // eye five units back along the z-axis and up three units, look at the 
            // origin, and define "up" to be in the y-direction.
            _viewMatrix = Matrix.LookAtLH(_location,
                _lookAt, new Vector3(0.0f, 0.0f, 1.0f));

            device.Transform.View = _viewMatrix;

            // For the projection matrix, we set up a perspective transform (which
            // transforms geometry from 3D view space to 2D viewport space, with
            // a perspective divide making objects smaller in the distance). To build
            // a perpsective transform, we need the field of view (1/4 pi is common),
            // the aspect ratio, and the near and far clipping planes (which define at
            // what distances geometry should be no longer be rendered).
            _projectionMatrix = Matrix.PerspectiveFovLH((float)(Math.PI / 4), _aspectRatio, 1.0f, 10000.0f);
            //_projectionMatrix = Matrix.OrthoLH(_width, _height, 1000.0f, -1000.0f);

            device.Transform.Projection = _projectionMatrix;
        }

        public Plane[] GetFrustum()
        {
            Matrix mat = Matrix.Multiply(_viewMatrix, _projectionMatrix);
            Plane[] frustum = new Plane[6];

            // Left
            frustum[0].A = mat.M11 + mat.M14;
            frustum[0].B = mat.M21 + mat.M24;
            frustum[0].C = mat.M31 + mat.M34;
            frustum[0].D = mat.M41 + mat.M44;

            // Right
            frustum[1].A = mat.M14 - mat.M11;
            frustum[1].B = mat.M24 - mat.M21;
            frustum[1].C = mat.M34 - mat.M31;
            frustum[1].D = mat.M44 - mat.M41;

            // Top
            frustum[2].A = mat.M14 - mat.M12;
            frustum[2].B = mat.M24 - mat.M22;
            frustum[2].C = mat.M34 - mat.M32;
            frustum[2].D = mat.M44 - mat.M42;
            
            // Bottom
            frustum[3].A = mat.M14 + mat.M12;
            frustum[3].B = mat.M24 + mat.M22;
            frustum[3].C = mat.M34 + mat.M32;
            frustum[3].D = mat.M44 + mat.M42;
            
            // Near
            frustum[4].A = mat.M14 + mat.M13;
            frustum[4].B = mat.M24 + mat.M23;
            frustum[4].C = mat.M34 + mat.M33;
            frustum[4].D = mat.M44 + mat.M43;
            
            // Far
            frustum[5].A = mat.M14 - mat.M13;
            frustum[5].B = mat.M24 - mat.M23;
            frustum[5].C = mat.M34 - mat.M33;
            frustum[5].D = mat.M44 - mat.M43;

            for (int i = 0; i < 6; i++)
            {
                frustum[i].Normalize();
            }

            return frustum;
        }
    }
}
