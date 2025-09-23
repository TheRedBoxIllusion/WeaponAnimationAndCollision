using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using System.Collections.Generic;
using System.ComponentModel;
using System;


namespace WeaponSystem
{
    public class Game1 : Game
    {
        private GraphicsDeviceManager _graphics;
        private SpriteBatch _spriteBatch;

        Weapon itemInHand;

        AnimationController animationController;

        List<Texture2D> spriteSheets;

        Texture2D mouseTexture;
        Texture2D boundingBoxTexture;

        SpriteFont text;

        Player player;

        MouseCollider mc;

        public Game1()
        {
            _graphics = new GraphicsDeviceManager(this);
            Content.RootDirectory = "Content";
            IsMouseVisible = true;

            animationController = new AnimationController();

            

            spriteSheets = new List<Texture2D>();
            player = new Player();

            itemInHand = new Weapon(animationController, player);

            mc = new MouseCollider();
        }

        protected override void Initialize()
        {
            // TODO: Add your initialization logic here

            base.Initialize();
        }

        protected override void LoadContent()
        {
            _spriteBatch = new SpriteBatch(GraphicsDevice);

            Texture2D weaponSpriteSheet = Texture2D.FromFile(_graphics.GraphicsDevice, AppDomain.CurrentDomain.BaseDirectory + "Fish-Knife.png");

            spriteSheets.Add(weaponSpriteSheet);

            mouseTexture = new Texture2D(_graphics.GraphicsDevice, 1, 1);
            mouseTexture.SetData(new Color[] { Color.Black});
            boundingBoxTexture = new Texture2D(_graphics.GraphicsDevice, 1, 1);
            boundingBoxTexture.SetData(new Color[] { Color.Red });

            text = Content.Load<SpriteFont>("File");
            // TODO: use this.Content to load your game content here
        }

        protected override void Update(GameTime gameTime)
        {
            if (GamePad.GetState(PlayerIndex.One).Buttons.Back == ButtonState.Pressed || Keyboard.GetState().IsKeyDown(Keys.Escape))
                Exit();

            if (Mouse.GetState().LeftButton == ButtonState.Pressed) {
                itemInHand.onLeftClick();
            }

            mc.updateCollider();

            if (itemInHand.itemAnimator != null)
            {
                ((INonAxisAlignedActiveCollider)itemInHand).nonAxisAlignedCollisionDetection(mc);
            }
            

            animationController.tickAnimation(gameTime.ElapsedGameTime.TotalSeconds);

            base.Update(gameTime);
        }

        protected override void Draw(GameTime gameTime)
        {
            GraphicsDevice.Clear(Color.CornflowerBlue);
            _spriteBatch.Begin();

            _spriteBatch.Draw(mouseTexture, new Rectangle(Mouse.GetState().X - 5, Mouse.GetState().Y - 6, 10, 12), Color.White);

            _spriteBatch.DrawString(text, Mouse.GetState().X + ", " + Mouse.GetState().Y, new Vector2(200, 50), Color.Red);

            ///*

            _spriteBatch.Draw(boundingBoxTexture, new Rectangle((int)(itemInHand.rotatedPoints[0].X + player.x - 2), (int)(itemInHand.rotatedPoints[0].Y + player.y - 2), 4, 4), Color.White);
            _spriteBatch.Draw(boundingBoxTexture, new Rectangle((int)(itemInHand.rotatedPoints[1].X + player.x - 2), (int)(itemInHand.rotatedPoints[1].Y + player.y - 2), 4, 4), Color.White);
            _spriteBatch.Draw(boundingBoxTexture, new Rectangle((int)(itemInHand.rotatedPoints[2].X + player.x - 2), (int)(itemInHand.rotatedPoints[2].Y + player.y - 2), 4, 4), Color.White);
            _spriteBatch.Draw(boundingBoxTexture, new Rectangle((int)(itemInHand.rotatedPoints[3].X + player.x - 2), (int)(itemInHand.rotatedPoints[3].Y + player.y - 2), 4, 4), Color.White);
            //*/

            for (int i = 0; i < animationController.animators.Count; i++)
            {
                
                Animator a = animationController.animators[i];
                Item owner = a.owner;
                
                _spriteBatch.Draw(spriteSheets[owner.spriteSheetID], new Rectangle((int)(player.x + a.currentPosition.xPos), (int)(player.y + a.currentPosition.yPos), (int)(owner.drawDimensions.width), (int)(owner.drawDimensions.height)), new Rectangle((int)owner.spriteSheetLocation.x, (int)owner.spriteSheetLocation.y, owner.sourceDimensions.width, owner.sourceDimensions.height), Color.White, (float)a.currentPosition.rotation, owner.origin, owner.spriteEffect, 0f);
            }
            _spriteBatch.End();

            base.Draw(gameTime);
        }
    }

    public class Item {
        public (double x, double y) spriteSheetLocation;
        public int spriteSheetID;
        public (int width, int height) sourceDimensions;
        public (int width, int height) drawDimensions;
        public Animator itemAnimator;
        public AnimationController animationController;
        public Vector2 origin;
        public double constantRotationOffset;

        public SpriteEffects spriteEffect;

        public int colliderWidth;
        public int colliderHeight;

        public Vector2 offsetFromEntity;

        public Player owner;

       

        public virtual void onLeftClick() {}
        public virtual void animationFinished() {
            itemAnimator = null;
        }
    }

    public class Weapon : Item, INonAxisAlignedActiveCollider {

        bool swungDownwardsLastIteration = false;

        public Vector2[] rotatedPoints { get; set; }

        public Vector2 rotationOrigin { get; set; }

        public int colliderWidth { get; set; }

        public int colliderHeight { get; set; }

        public (double x, double y) location { get; set; }

        public Player owner { get; set; }

        public Animator itemAnimator { get; set; }

        public bool isActive { get; set; }

        public Weapon(AnimationController ac, Player owner) {

            animationController = ac;

            this.owner = owner;

            location = (owner.x, owner.y);

            constantRotationOffset = -Math.PI / 4;

            spriteSheetLocation = (0, 0);
            origin = new Vector2(-2f, 18f);
            rotationOrigin = new Vector2(-2, 18f);
            sourceDimensions = (16, 16);
            drawDimensions = (32, 32);

            rotatedPoints = new Vector2[4];


            colliderWidth = 4;
            colliderHeight = 32;

        }
        //Adjusted to define the rectangular vertices only once, this should be a bit more efficient
        public override void onLeftClick()
        {
            if (itemAnimator == null)
            {
                isActive = true;
                if (!swungDownwardsLastIteration)
                {
                    spriteEffect = SpriteEffects.None;
                    origin = new Vector2(-2f, 18f);
                    location = (owner.x - origin.X, owner.y - origin.Y);
                    constantRotationOffset = -Math.PI / 4;

                    float initialRotation = (float)-Math.PI / 6;
                    itemAnimator = new Animator(animationController, this, 0.4, (0, 0, initialRotation), (0, 0, 2 * Math.PI / 3), constantRotationOffset);
                    swungDownwardsLastIteration = true;

                    initialiseColliderVectors(1, initialRotation);


                }
                else {

                    spriteEffect = SpriteEffects.FlipVertically;
                    origin = new Vector2(-2f, -2f);
                    location = (owner.x - origin.X, owner.y - origin.Y);
                    constantRotationOffset = Math.PI / 4;

                    float initialRotation = (float)-Math.PI / 6;
                    itemAnimator = new Animator(animationController, this, 0.15, (0, 0, initialRotation), (0, 0, - Math.PI / 2), constantRotationOffset);
                    swungDownwardsLastIteration = false;
                    initialiseColliderVectors(-1, initialRotation);

                }
                animationController.addAnimator(itemAnimator);
            }
        }

        public override void animationFinished()
        {
            isActive = false;
            itemAnimator = null;
        }
        
        /*
        public void nonAxisAlignedCollisionDetection((double X, double Y) player, int horizontalDirection, Rectangle externalCollider) {
            double theta = itemAnimator.currentPosition.rotation;
            Rectangle externalColliderInLocalSpace = new Rectangle((int)(externalCollider.Center.X - (player.X + horizontalDirection * -origin.X)), (int)(externalCollider.Center.Y - (player.Y - origin.Y)), externalCollider.Width, externalCollider.Height);
            //find the direction of the secondary object and determine which axis to project onto. I can definitely project solely onto an x-y plane given that I'm working with rectangular colliders for now.
            Vector2 seperatingAxis = calculateSeperationAxis(externalColliderInLocalSpace);
            
            //From the seperating axis, project the collider's shadow onto that axis, then see if there's a gap between the closest points... How shall I do that. Based on the axis, I can tell 
            //The center of the weapon is at 0,0 so that's to note. The seperating axis indicates what corner of the local and external colliders to use. I can take the weapons rectangle, then use a matrix transformation to rotate them, then find the point that has the greatest value along the seperating axis, which is also what it would be like projected, so i can ignore having to do vector dot products and merely take the appropriate component out of the transformed vectors.
            calculateRotation((float)itemAnimator.currentChange.rotation);

            //Multiply the vectors by the seperating axis to get the proj onto that axis, I can then take the largest (or most negative) one and use that for the shadow. But how do I determine if the two shadows are overlapping? I can get the shadow length,
            //I can calculate the distance (based on the externalColliderInLocalSpace) it's shadow is literally just half the dimension in whatever axis, and the distance is the position of the collider in local space.

           
            
            double shadow = 0;
            
            for (int i = 0; i < rotatedPoints.Length; i++) {
                Vector2 point = rotatedPoints[i] * seperatingAxis;
                if (seperatingAxis.X != 0)
                {
                    if (shadow < point.X) shadow = point.X;
                }
                else if (seperatingAxis.Y != 0) {
                    if (shadow < point.Y) shadow = point.Y;
                }
            }
            


            System.Diagnostics.Debug.WriteLine("Distance: " + externalColliderInLocalSpace.X + " * " + seperatingAxis.X);
            System.Diagnostics.Debug.WriteLine("Shadow: " + shadow + " + " + externalColliderInLocalSpace.Width / 2);
            System.Diagnostics.Debug.WriteLine("Distance: " + externalColliderInLocalSpace.Y + " * " + seperatingAxis.Y);
            System.Diagnostics.Debug.WriteLine("Shadow: " + shadow + " + " + externalColliderInLocalSpace.Height / 2);

            System.Diagnostics.Debug.WriteLine("Location: " + player.X + ", " + player.Y);
            System.Diagnostics.Debug.WriteLine("Collider location: " + externalCollider.X + ", " + externalCollider.Y);
            

            if (seperatingAxis.X != 0)
            {
                if (externalColliderInLocalSpace.X * seperatingAxis.X < shadow + externalColliderInLocalSpace.Width / 2) {
                    //Has collided
                    System.Diagnostics.Debug.WriteLine("Collided in the X axis");
                }
            }
            else if (seperatingAxis.Y != 0)
            {
                if (externalColliderInLocalSpace.Y * seperatingAxis.Y < shadow + externalColliderInLocalSpace.Height / 2)
                {
                    //Has collided
                    System.Diagnostics.Debug.WriteLine("Collided in the Y axis");
                }
            }


        }

        public Vector2 calculateSeperationAxis(Rectangle externalColliderInLocalSpace) {
            Vector2 seperatingAxis;
            if (externalColliderInLocalSpace.X > 0) //Is to the right
            {
                if (externalColliderInLocalSpace.Y > 0) //Is downwards
                {
                    //Determine which axis is overlapping more, then use that to determine the appropriate seperatingAxis. Ensure that it's the one with no/little overlap. EG using the x-axis for things on top of eachother will produce false positives
                    if (externalColliderInLocalSpace.Y - externalColliderInLocalSpace.X > 0)
                    { //Is more below than to the right. So use the y axis to determine collision.
                        seperatingAxis = new Vector2(0, 1);
                        System.Diagnostics.Debug.WriteLine("One");
                    }
                    else
                    {
                        seperatingAxis = new Vector2(1, 0);
                        System.Diagnostics.Debug.WriteLine("Two");
                    }
                }
                else //Is upwards
                {
                    if (externalColliderInLocalSpace.X + externalColliderInLocalSpace.Y > 0) //More right than up
                    {
                        seperatingAxis = new Vector2(1, 0);
                        System.Diagnostics.Debug.WriteLine("Three");
                    }
                    else
                    {
                        seperatingAxis = new Vector2(0, -1);
                        System.Diagnostics.Debug.WriteLine("Four");
                    }

                }
            }
            else  //Is to the left
            {
                if (externalColliderInLocalSpace.Y > 0) //Is downwards
                {
                    //Determine which axis is overlapping more, then use that to determine the appropriate seperatingAxis. Ensure that it's the one with no/little overlap. EG using the x-axis for things on top of eachother will produce false positives
                    if (externalColliderInLocalSpace.Y + externalColliderInLocalSpace.X > 0)
                    { //Is more below than to the left. So use the y axis to determine collision.
                        seperatingAxis = new Vector2(0, 1);
                        System.Diagnostics.Debug.WriteLine("Funf");
                    }
                    else
                    {
                        seperatingAxis = new Vector2(-1, 0);
                        System.Diagnostics.Debug.WriteLine("Sechs");
                    }
                }
                else //Is upwards
                {
                    if (externalColliderInLocalSpace.X - externalColliderInLocalSpace.Y < 0) //More left than up
                    {
                        seperatingAxis = new Vector2(-1, 0);
                        System.Diagnostics.Debug.WriteLine("Seben");
                    }
                    else
                    {
                        seperatingAxis = new Vector2(0, -1);
                        System.Diagnostics.Debug.WriteLine("acht");
                    }

                }

            }

            return seperatingAxis;
        }

        public void calculateRotation(float rotation)
        {
            topLeft = Vector2.RotateAround(topLeft, new Vector2(0, 0), rotation);
            topRight = Vector2.RotateAround(topRight, new Vector2(0, 0), rotation);
            bottomLeft = Vector2.RotateAround(bottomLeft, new Vector2(0, 0), rotation);
            bottomRight = Vector2.RotateAround(bottomRight, new Vector2(0, 0), rotation);
        }
        //*/



        private void initialiseColliderVectors(int multiplier, float initialRotation) {
            rotatedPoints[0] = new Vector2(-colliderWidth, -colliderHeight) - rotationOrigin; //The rotation origin doesn't adjust with the origin that is used for drawing. This is because the drawing system and the collision system operate under different grid spacesgit
            rotatedPoints[1] = new Vector2(0, -colliderHeight) - rotationOrigin;
            rotatedPoints[2] = new Vector2(-colliderWidth, 0) - rotationOrigin;
            rotatedPoints[3] = new Vector2(0, 0) - rotationOrigin;



            rotatedPoints[0] *= multiplier;
            rotatedPoints[1] *= multiplier;
            rotatedPoints[2] *= multiplier;
            rotatedPoints[3] *= multiplier;

            ((INonAxisAlignedActiveCollider)this).calculateRotation(initialRotation);
        }
        
    }

    public class Animator {
        public double duration;
        public double elapsedTime;
        public double maxDuration;
        public (double xPos, double yPos, double rotation) initialPosition;
        public (double xPos, double yPos, double rotation) currentPosition;
        public (double xPos, double yPos, double rotation) finalPosition;
        
        public Item owner;

        public (double xPos, double yPos, double rotation) currentChange;
        double constantRotationOffset;
        


        public AnimationController animationController;

        public Animator(AnimationController ac, Item owner, double duration, (double xPos, double yPos, double rotation) initialPosition, (double xPos, double yPos, double rotation) finalPosition, double constantRotationOffset) {
            animationController = ac;
            this.owner = owner;

            this.duration = 0;
            maxDuration = duration;
            initialPosition.rotation += constantRotationOffset;
            finalPosition.rotation += constantRotationOffset;

            this.constantRotationOffset = constantRotationOffset;


            this.initialPosition = initialPosition;
            currentPosition = initialPosition;
            this.finalPosition = finalPosition;
            
        }

        public void tick(double elapsedTime)
        {
            duration += elapsedTime;
            this.elapsedTime = elapsedTime;
            if (duration >= maxDuration)
            {
                animationController.removeAnimator(this);
                owner.animationFinished();
            }

            currentChange.xPos = linearInterpolation(initialPosition.xPos, finalPosition.xPos);
            currentChange.yPos = linearInterpolation(initialPosition.yPos, finalPosition.yPos);
            currentChange.rotation = linearInterpolation(initialPosition.rotation, finalPosition.rotation);
            
            

            currentPosition = (currentPosition.xPos + currentChange.xPos, currentPosition.yPos + currentChange.yPos, currentPosition.rotation + currentChange.rotation);

            
            



        }

        public double linearInterpolation(double initialValue, double finalValue)
        {
            double difference = finalValue - initialValue;
            double differencePerSecond = difference / maxDuration;
            double linearlyInterpolatedValue = differencePerSecond * elapsedTime;// + initialValue; //Altered to calculate the change, then do the addition of the initial value when defining the current position. This allows for the change in a frame to be calculated for later efficiency purposes

            return linearlyInterpolatedValue;
        }    
    
    }

    public class AnimationController {
        public List<Animator> animators;

        public AnimationController() {
            animators = new List<Animator>();
        }

        public void addAnimator(Animator animator) {
            animators.Add(animator);
            
        }
        public void removeAnimator(Animator animator) {
            animators.Remove(animator);
        }

        public void tickAnimation(double elapsedTime) {
            for (int i = 0; i < animators.Count; i++) {
                animators[i].tick(elapsedTime);
            }
        }
    }

    public class Player {
        public double x = 50;
        public int y = 50;
    }

    public class MouseCollider : IPassiveCollider {
        public Rectangle collider { get; set; }
        public bool isActive { get; set; }

        public MouseCollider() {
            isActive = true;
            updateCollider();
        }

        public void updateCollider() {
            collider = new Rectangle(Mouse.GetState().X - 5, Mouse.GetState().Y - 6, 10, 12);
        }

    }


    public interface ICollider {
        bool isActive { get; set; }
    

        public void onCollision(ICollider otherCollider) {
        
        }
    }

    public interface IActiveCollider : ICollider {

        Player owner { get; set; }
        public virtual void calculateCollision(IPassiveCollider externalCollider) {
        
        }
    }

    public interface INonAxisAlignedActiveCollider : IActiveCollider {

        public Vector2[] rotatedPoints { get; set; }

        public Vector2 rotationOrigin { get; set; }

        public int colliderWidth { get; set; }
        public int colliderHeight { get; set; }

        public (double x, double y) location { get; set; }

        public Animator itemAnimator { get; set; }



        public void calculateCollision(IPassiveCollider externalCollider) {
            
            if (isActive)
            {
                nonAxisAlignedCollisionDetection(externalCollider);
            }

        }

        public void nonAxisAlignedCollisionDetection(IPassiveCollider externalCollider)
        {
            double theta = itemAnimator.currentPosition.rotation;
            

            Rectangle externalColliderInLocalSpace = new Rectangle((int)(externalCollider.collider.Center.X - location.x), (int)(externalCollider.collider.Center.Y - location.y), externalCollider.collider.Width, externalCollider.collider.Height);

            
            //find the direction of the secondary object and determine which axis to project onto. I can definitely project solely onto an x-y plane given that I'm working with rectangular colliders for now.
            Vector2 seperatingAxis = calculateSeperationAxis(externalColliderInLocalSpace);
            //From the seperating axis, project the collider's shadow onto that axis, then see if there's a gap between the closest points... How shall I do that. Based on the axis, I can tell 
            //The center of the weapon is at 0,0 so that's to note. The seperating axis indicates what corner of the local and external colliders to use. I can take the weapons rectangle, then use a matrix transformation to rotate them, then find the point that has the greatest value along the seperating axis, which is also what it would be like projected, so i can ignore having to do vector dot products and merely take the appropriate component out of the transformed vectors.
            calculateRotation((float)itemAnimator.currentChange.rotation);

            //Multiply the vectors by the seperating axis to get the proj onto that axis, I can then take the largest (or most negative) one and use that for the shadow. But how do I determine if the two shadows are overlapping? I can get the shadow length,
            //I can calculate the distance (based on the externalColliderInLocalSpace) it's shadow is literally just half the dimension in whatever axis, and the distance is the position of the collider in local space.

            double shadow = 0;
            for (int i = 0; i < rotatedPoints.Length; i++)
            {
                
                if (seperatingAxis.X != 0)
                {
                    if (shadow < rotatedPoints[i].X) shadow = rotatedPoints[i].X;
                }
                else if (seperatingAxis.Y != 0)
                {
                    if (shadow < rotatedPoints[i].Y) shadow = rotatedPoints[i].Y;
                }
            }

            


            if (seperatingAxis.X != 0)
            {
                if (externalColliderInLocalSpace.X * seperatingAxis.X < shadow + externalColliderInLocalSpace.Width / 2)
                {
                    //Has collided
                    System.Diagnostics.Debug.WriteLine("Collided in the X axis");
                }
            }
            else if (seperatingAxis.Y != 0)
            {
                if (externalColliderInLocalSpace.Y * seperatingAxis.Y < shadow + externalColliderInLocalSpace.Height / 2)
                {
                    //Has collided
                    System.Diagnostics.Debug.WriteLine("Collided in the Y axis");
                }
            }


        }

        public Vector2 calculateSeperationAxis(Rectangle externalColliderInLocalSpace)
        {
            Vector2 seperatingAxis;
            if (externalColliderInLocalSpace.X > 0) //Is to the right
            {
                if (externalColliderInLocalSpace.Y > 0) //Is downwards
                {
                    //Determine which axis is overlapping more, then use that to determine the appropriate seperatingAxis. Ensure that it's the one with no/little overlap. EG using the x-axis for things on top of eachother will produce false positives
                    if (externalColliderInLocalSpace.Y - externalColliderInLocalSpace.X > 0)
                    { //Is more below than to the right. So use the y axis to determine collision.
                        seperatingAxis = new Vector2(0, 1);
                    }
                    else
                    {
                        seperatingAxis = new Vector2(1, 0);
                    }
                }
                else //Is upwards
                {
                    if (externalColliderInLocalSpace.X + externalColliderInLocalSpace.Y > 0) //More right than up
                    {
                        seperatingAxis = new Vector2(1, 0);
                    }
                    else
                    {
                        seperatingAxis = new Vector2(0, -1);
                    }

                }
            }
            else  //Is to the left
            {
                if (externalColliderInLocalSpace.Y > 0) //Is downwards
                {
                    //Determine which axis is overlapping more, then use that to determine the appropriate seperatingAxis. Ensure that it's the one with no/little overlap. EG using the x-axis for things on top of eachother will produce false positives
                    if (externalColliderInLocalSpace.Y + externalColliderInLocalSpace.X > 0)
                    { //Is more below than to the left. So use the y axis to determine collision.
                        seperatingAxis = new Vector2(0, 1);
                    }
                    else
                    {
                        seperatingAxis = new Vector2(-1, 0);
                    }
                }
                else //Is upwards
                {
                    if (externalColliderInLocalSpace.X - externalColliderInLocalSpace.Y < 0) //More left than up
                    {
                        seperatingAxis = new Vector2(-1, 0);
                    }
                    else
                    {
                        seperatingAxis = new Vector2(0, -1);
                    }

                }

            }

            return seperatingAxis;
        }

        public void calculateRotation(float rotation)
        {
            rotatedPoints[0] = Vector2.RotateAround(rotatedPoints[0], new Vector2(0, 0), rotation);
            rotatedPoints[1] = Vector2.RotateAround(rotatedPoints[1], new Vector2(0, 0), rotation);
            rotatedPoints[2] = Vector2.RotateAround(rotatedPoints[2], new Vector2(0, 0), rotation);
            rotatedPoints[3] = Vector2.RotateAround(rotatedPoints[3], new Vector2(0, 0), rotation);
        }


    }

    public interface IPassiveCollider : ICollider {

        Rectangle collider { get; set; }
        //External colliders are colliders that don't compute their own collisions, they only react to collisions. Lets say that monsters have IExternalColliders, when the player collides with the monster, the collision function is run, but the monster doesn't also compute if it collided.
        //I think this will just make it a bit easier to seperate player based colliders from entity colliders. Weapons, including arrows, will have actual colliders that compute collisions with external colliders. This way weapons can 
    } 
}
