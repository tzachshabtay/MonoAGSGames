using System;
using System.Collections.Generic;
using AGS.API;

namespace LastAndFurious
{
    public class VehicleAIPathBased : VehicleControl
    {
        List<AIPathNode> _pathNodes;
        bool _targetValid;
        Vector2 _targetPos;
        Vector2 _targetDir;
        float _targetCheckRadius;
        float _targetThreshold;
        float _targetSpeedHint;
        AIPathNode _currentNode;

        public VehicleAIPathBased(IGame game, List<AIPathNode> pathNodes)
            : base(game)
        {
            _pathNodes = pathNodes;
            _targetValid = false;
        }

        private bool testShouldChooseNewTarget()
        {
            if (!_targetValid)
                return true;
            // Choose next path node if inside the check radius for current one, or closer to next one.
            if (_currentNode != null)
            {
                AIPathNode prevNode = _currentNode.prev;
                AIPathNode nextNode = _currentNode.next;
                if (nextNode != null &&
                    (prevNode == null || Vectors.Distance(_veh.Position, prevNode.pt) > Vectors.Distance(_currentNode.pt, prevNode.pt)) &&
                    Vectors.Distance(_veh.Position, nextNode.pt) < Vectors.Distance(_currentNode.pt, nextNode.pt))
                    return true;
            }
            return Vectors.Distance(_veh.Position, _targetPos) <= _targetCheckRadius;
        }

        private bool chooseNewTarget()
        {
            if (_pathNodes != null && _pathNodes.Count > 0)
            {
                if (_currentNode != null && _currentNode.pt != null)
                {
                    _currentNode = _currentNode.next;
                }
                else
                {
                    _currentNode = _pathNodes[0]; // TODO: find nearest?
                }
                _targetPos = _currentNode.pt;
                _targetCheckRadius = _currentNode.radius;
                _targetThreshold = _currentNode.threshold;
                _targetSpeedHint = _currentNode.speed;
                _targetValid = true;
            }
            else
            { // TODO??
            }
            return true;
        }

        private void driveToTheTarget()
        {
            // Turn into target's direction
            if (!_targetValid)
                return;
            _targetDir = Vector2.Subtract(_targetPos, _veh.Position);

            float angleThreshold = 0f;
            if (!_targetDir.IsZero())
                angleThreshold = (float)Math.Atan(_targetThreshold / _targetDir.Length);
            float angleBetween = Vectors.AngleBetween(_veh.Direction, _targetDir);
            if (angleBetween >= -angleThreshold && angleBetween <= angleThreshold)
            {
                _veh.SteeringWheelAngle = 0f;
            }
            else
            {
                if (angleBetween > 0.0)
                    _veh.SteeringWheelAngle = SteeringAngle;
                else
                    _veh.SteeringWheelAngle = -SteeringAngle;
            }

            _veh.Brakes = 0.0f;
            if (_targetSpeedHint < 0.0)
            {
                _veh.Accelerator = 1.0f;
            }
            else
            {
                float speed = _veh.Velocity.Length; // TODO: need only rolling velocity
                if (speed < _targetSpeedHint)
                {
                    _veh.Accelerator = 1.0f;
                }
                else if (speed > _targetSpeedHint)
                {
                    _veh.Accelerator = 0.0f;
                    _veh.Brakes = 1.0f;
                }
            }
        }

        public override void Run(float deltaTime)
        {
            if (this.testShouldChooseNewTarget())
            {
                if (!this.chooseNewTarget())
                    return;
            }
            this.driveToTheTarget();
        }
    }
}
