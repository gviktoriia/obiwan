using System;
using UnityEngine;
using UnityEngine.InputSystem;
using Unity.Netcode;

namespace Assignment3.SampleSolution
{
    public class ThumbstickSteeringNavigation : NetworkBehaviour
    {
        public enum InputMapping
        {
            PositionControl,
            VelocityControl,
            AccelerationControl
        }

        public InputMapping inputMapping = InputMapping.PositionControl;

        public Transform head;

        public InputActionReference steeringAction;
        public InputActionReference rotationAction;

        // Steering - General
        // Implement
        private Vector3 startingPosition;
        [Range(0.1f, 10.0f)] public float steeringSpeed = 2f;
        private float maxAcceleration = 2;
        private float breakingFactor = 10;
        private float maxVelocity = 5;
        private Vector3 currentVelocity = Vector3.zero;

        // Rotation
        [Range(1.0f, 180.0f)] // Draws a slider with range in the inspector 
        public float rotationSpeed; // In angle degrees per second

        // Groundfollowing
        public LayerMask groundLayerMask;
        private RaycastHit hit;

        private void Start()
        {
            // Reference point for computing position control
            startingPosition = transform.position;
        }

        void Update()
        {
            if (!IsOwner) return;
            ApplyDisplacement();
            ApplyRotation();
            ApplyGroundfollowing();
        }

        [ServerRpc]
        private void UpdatePositionServerRpc(Vector3 newPosition)
        {
            transform.position = newPosition;
        }

        private void ApplyDisplacement()
        {
            Vector2 input = steeringAction.action.ReadValue<Vector2>();
            switch (inputMapping)
            {
                case InputMapping.PositionControl:
                    PositionControl(input);
                    break;
                case InputMapping.VelocityControl:
                    VelocityControl(input);
                    break;
                case InputMapping.AccelerationControl:
                    AccelerationControl(input);
                    break;
            }
        }

        private void PositionControl(Vector2 input)
        {
            // Implement
            Vector3 headOffset = transform.position - head.transform.position;
            Vector3 displacement = new Vector3(input.x, 0, input.y) * 10;

            // Rotate displacement direction by rotation matrix of navigation node
            displacement = Matrix4x4.TRS(Vector3.zero, head.transform.rotation, Vector3.one)
                .MultiplyPoint(displacement);
            transform.position = startingPosition + displacement - headOffset;
        }

        private void VelocityControl(Vector2 input)
        {
            // Implement
            float speedFactor = input.magnitude;

            // input.x and .y are mapped to x and z axis and scaled by speed and time
            Vector3 displacement = new Vector3(input.x, 0, input.y).normalized *
                                   (steeringSpeed * speedFactor * Time.deltaTime);
            displacement = Matrix4x4.TRS(Vector3.zero, head.transform.rotation, Vector3.one)
                .MultiplyPoint(displacement);
            transform.position += displacement;
        }

        private void AccelerationControl(Vector2 input)
        {
            // Implement
            float inputMagnitude = input.magnitude;
            Vector3 dir = new Vector3(input.x, 0, input.y);
            float acceleration = 0;
            acceleration = inputMagnitude * maxAcceleration;
            currentVelocity += acceleration * Time.deltaTime * dir.normalized;
            if (currentVelocity.magnitude > maxVelocity)
            {
                currentVelocity = maxVelocity * currentVelocity.normalized;
            }

            Vector3 displacement = Matrix4x4.TRS(Vector3.zero, transform.rotation, Vector3.one)
                .MultiplyPoint(currentVelocity);
            transform.position += displacement;
        }

        private void ApplyRotation()
        {
            // Implement
            float turnfactor = rotationAction.action.ReadValue<Vector2>().x; // horizontal axis of thumbstick
            transform.RotateAround(head.position, Vector3.up, turnfactor * rotationSpeed * Time.deltaTime);
        }

        private void ApplyGroundfollowing()
        {
            // Solution
            // falling
            if (Physics.Raycast(head.position, -transform.up, out hit,
                    Single.PositiveInfinity, groundLayerMask))
            {
                transform.position = new Vector3(transform.position.x, hit.point.y, transform.position.z);
            }
            // rising
            else if (Physics.Raycast(head.position, transform.up, out hit,
                         Single.PositiveInfinity, groundLayerMask))
            {
                transform.position = new Vector3(transform.position.x, hit.point.y, transform.position.z);
            }
        }
    }
}