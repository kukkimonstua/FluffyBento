-----------------------------------------------------
	Very Animation
	Copyright (c) 2017 AloneSoft
	http://alonesoft.sakura.ne.jp/
	mail: support@alonesoft.sakura.ne.jp
	twitter: @AlSoSupport
-----------------------------------------------------

Thank you for purchasing "Very Animation".


[How to Update] 
Remove old VeryAnimation folder
Import VeryAnimation


[Documentation]
Assets/VeryAnimation/Documents


[Demo]
Assets/VeryAnimation/Demo


[Support]
mail: support@alonesoft.sakura.ne.jp
twitter: @AlSoSupport


[Update History]
Version 1.1.16
- ADD : Editor Window : Added a function to display Selection items on the Scene Window
- ADD : Timeline : Added Dummy Object display mode and change default settings
- FIX : Hierarchy : Humanoid Hips has no editing target, so display flag is disabled by default
- FIX : Hierarchy : Change the operation button 'Head' of Humanoid to 'Face'
- FIX : Unity2019.3 : Fixed display issue

Version 1.1.15
- ADD : Tools/Root Motion : Added the ability to edit Motion curves
- ADD : Animator IK : Added a warning when IK does not work properly due to changes in the scale
- FIX : Humanoid : Error display when bone set in Avatar has been deleted
- FIX : Tools/Keyframe Reduction : Fixed an issue where Parameter Related curves and Generic Root curves were not reduced
- FIX : Gizmo : Fixed a problem that the handle display disappears when there is no focused window.
- FIX : Hierarchy : Fixed an issue where an error may appear in the console when selecting objects in Project

Version 1.1.14
- ADD : Unity2019.2 support
- ADD : Animation Window : Sorting and filtering by Property selection state
- FIX : Root Correction : Fixed the problem that RootQ is reversed in existing motion editing etc.
- FIX : IK : Fix parent space behavior
- FIX : Settings : Organize items
- FIX : Animator IK : Head : Change the setting so that the total of Weight becomes 1
- FIX : Hierarchy : Modified to show selected objects in the frame
- FIX : Timeline : Fixed an issue where the Foot IK update was incorrect when the Animator Controller was not set
- FIX : Unity2019.3 error

Version 1.1.13p1
- FIX : Preview : Root Motion related fixes, adding information
- FIX : Fixed the Hand tool to work
- FIX : Corrected the behavior of "Edit/Frame Selected" when nothing is selected
- FIX : Fixed to continue editing if there is a change in hierarchy while editing
- FIX : Addressing an issue where the Animation Window and the Hierarchy Window end immediately after editing starts if they belong to the same Dock Area
- FIX : Legacy Animation : Fixed the problem that returns to 0 frame at the last frame if the Clip is set to loop

Version 1.1.13
- ADD : Options : Collision function added
- ADD : IK : Add Sync button
- FIX : Preview : Modified to apply Transform information in Scene to Preview as it is

Version 1.1.12
- ADD : Tools : Create New Keyframe
- FIX : Keyframe Reduction : The bug that the reduction fails when the name of GameObject contains '.'.
- FIX : Shortcuts : Since the 2019.1.4f1 H key is no longer Global, the display switching key is changed to the H key again
- FIX : Tools : Keyframe Reduction : Fixed a bug that the specification of Error value was not reflected
- FIX : VA Tools : Fixed bug that the object not active in the whole process is not processed
- FIX : Fixed a problem that does not start with an error

Version 1.1.11p1
- FIX : Fixed that other scripts do not malfunction while editing
- FIX : DaeExporter : Change settings to Importer as much as possible
- FIX : Keyframe Reduction : Bug fix that rotation is not reduced normally

Version 1.1.11
- ADD : Tools : Generic Root Motion
- FIX : The procedure has been changed because there is data that Generic Root Motion does not work properly in the procedure up to the previous version
- FIX : DaeExporter : Fixed output problems with models with different Bindpose and Prefab pose
- FIX : Keyframe Reduction : Fix the problem that the rotation curve can not be reduced due to an error
- FIX : Create New Clip : Mirror : Humanoid TDOF mirror bug fix

Version 1.1.10p1
- FIX : SaveSettings : Fixed an issue where Overrides information may become abnormal if there is Save Settings on Prefab
- FIX : Assembly Definition File
- FIX : Speeding up line drawing of Skeleton

Version 1.1.10
- ADD : Extra functions/Trail of root position
- FIX : Extra functions : Foldout support for display
- FIX : BlendShape : Fix the problem that an error occurs when there is more than one same name
- FIX : Fixed a bug that could not reflect changes in AnimationClip being edited
- FIX : Editing while playing : Fixed the problem that Time is shifted when Speed is not 1.

Version 1.1.9p2
- FIX : Fix the problem that icon acquisition error such as Particle System is displayed in the log
- FIX : SaveData : Enhanced error checking of loading process

Version 1.1.9p1
- FIX : EditorWindow : Modify UI for Blend Pose, Muscle Group, Blend Shape, Selection

Version 1.1.9
- ADD : Assembly Definition File : support
- ADD : Unity2019.1 : Supports uninstallation of Timeline
- ADD : Onion Skinning
- FIX : Timeline : IK : Space Parent 

Version 1.1.8
- ADD : Unity2019.1 support
- ADD : Shortcut Manager : support
- ADD : Tools : Add "Combine"
- FIX : Shortcut : Change of some shortcuts (in order to conflict with shortcuts of Unity 2019.1 or later)

Version 1.1.7
- ADD : Editor Window : Corresponds so that the docking or floating Window style can be selected by setting
- ADD : Generic Mirror : Added Scale's mirror function
- FIX : Create New Clip : Fixed an issue where updates to Animator Controller may not be saved
- FIX : AnimatorIK : IK accuracy correction
- FIX : IK : Fix to make IK work by changing in Animation Window

Version 1.1.6p1
- FIX : Keyframe Reduction : Fixed an issue that may not work properly with Avatar of Humanoid created by other than Model Importer
- FIX : Keyframe Reduction : Added "Root and IK Goal Curves" flag to invalidate reduction of some curves for Foot IK
- FIX : Export : Fixed the problem outputted at pose when editing started

Version 1.1.6
- ADD : BlendShape : Addition of functions that save all values with a name and can be reused
- FIX : Fixed an issue that BlendShape information etc. was not changed by the function to change the edit target by double-clicking while editing

Version 1.1.5p1
- FIX : Timeline : Fixed an issue that did not return to the pre-edit pose at the end of editing
- FIX : Timeline : Correction of movement mainly of Local space and Parent space of AnimatorIK and OriginalIK

Version 1.1.5
- ADD : Synchronize Animation : Function to edit while previewing multiple targets
- ADD : Add UI to select the AnimationClip to be edited to the screen before the start of editing
- ADD : Settings : Japaneseization of Tooltip
- ADD : Double-clicking another GameObject during editing adds a function to change the editing target
- ADD : Save and restore last selected AnimationClip added
- FIX : Fixed a problem that the position shifted at the end of editing by pause
- FIX : Fixed an issue where an event was called by Animator.Update by internal calculation
- FIX : Speed up

Version 1.1.4
- ADD : Unity2018.3 support
- ADD : Added log display at end of edit on error
- ADD : Dummy Object color change effective flag addition
- FIX : Fixed a problem that position shifted due to pause editing
- FIX : Add warning about Root Motion relation
- FIX : Save data of mirror bone
- FIX : Unity2019.1 error

Version 1.1.3p2
- FIX  : ControlWindow/Selection : Move select : Fix Active Game Object
- FIX : Pivot Center
- FIX : OriginalIK : Basic : Weight initial value was corrected to 1.0
- FIX : OriginalIK : Reset

Version 1.1.3p1
- ADD : Add message display on unsupported version of Unity
- ADD : ControlWindow/Selection : Move select
- FIX : ControlWindow/Humanoid : Correction of range selection bug in Window scroll
- FIX : Generic Mirror
- FIX : Pivot Center
- FIX : speedup

Version 1.1.3
- ADD : Mirror : Mirror target manual setting correspondence
- ADD : EditorWindow : Rotation display added to Humanoid's bone
- ADD : Tools/Create new clip : Add mirror
- ADD : Tools/Keyframe Reduction
- ADD : ControlWindow : Hierarchy : Add Mirror target button
- ADD : EditorWindow : MuscleGroup, BlendShape : Add Mirror target button
- ADD : Tools/Export : Add Active Only
- ADD : ToolWindow/Reset Pose, Template Pose : Added correspondence of BlendShape
- FIX : EditorWindow : Change TDOF display of Humanoid's bone to Position
- FIX : Tools/Copy : Correction of copy process
- FIX : Tools/Cleanup : BlendShape processing correction
- FIX : Pose/Mirror : Process modification
- FIX : Generic Mirror : Modify mirror bone search
- FIX : BlendShape Mirror : Fixed problem not working on Timeline
- FIX : Humanoid TDOF : Handle local axis correction
- FIX : BlendShape : Correct reset value
- FIX : Mirror : Process modification

Version 1.1.2p3
- FIX : Options/FootIK : Correction of curve generation part
- FIX : Tools/Create new clip : Fixed problem that editing mode ends when overwriting files

Version 1.1.2p2
- FIX : Fixed an issue that does not start with an error if additional bones etc. are created in the model

Version 1.1.2p1
- FIX : Correction judgment on creation of new animation curve in Reset etc
- FIX : Mirror : Generic Mirror reverse rotation correction
- FIX : Tools/Rotation Curve Interpolation : Euler Angles (Quaternion) -> Quaternion

Version 1.1.2
- ADD : Tools/Range IK
- ADD : Animator IK, Original IK : Corresponds to copying and pasting target information
- ADD : Animator IK : Addition of heel operating handle to Foot
- FIX : Mirror : Generic Mirror
- FIX : Mac : Fixed shortcut key bug in Mac editor
- FIX : Humanoid : Fixed an issue where Bind and Prefab button behavior and Animator IK operation were abnormal when the bone scale was not 1
- FIX : Timeline : Animator IK : Change the behavior in Parent space from dummy object space motion to object space motion
- FIX : Root Correction : Fixed a problem that Root correction was not working with Copy & Paste
- FIX : Animator IK : Fix swivel value acquisition when limbs are stretched straight
- FIX : Animator IK, Original IK : Omit the Fixed function
- FIX : Settings : Change dummy object display setting default
- FIX : Unity2018.3 : Error correction
- FIX : Obsolete API
- FIX : speedup

Version 1.1.1p4
- FIX : Fixed an issue that may not work due to an error when multiple identical names exist in BlendShape

Version 1.1.1p3
- FIX : Fixed an issue that does not work with errors when there are multiple GameObjects with the same name in the same hierarchy

Version 1.1.1p2
- FIX : Responding to problems that do not work due to BindPose acquisition processing error
- FIX : IK : Correspond so that Bone is selected when list is selected in IK invalid state
- FIX : Correction of animation curve update processing
- FIX : Changed so that Animation Window keyframe selection is not canceled by animation curve update

Version 1.1.1p1
- ADD : Tools/Clearnup : Add BlendShape
- FIX : Tools/Clearnup : Change Eye, Jaw and Toe individually
- FIX : Mirror : Fixed an issue that sometimes does not work due to operation with Animation Window
- FIX : MuscleGroup/Part : Independence from Head with Eye and Jaw as Face
- FIX : Unity2018.2 : Fixed problem that "Open Animation Window" button does not work

Version 1.1.1
- ADD : Unity2018.2 support
- ADD : Muscle Group : Change Foldout to Bone selection button of target Node
- ADD : Blend Shape : Change Foldout to Bone selection button of target Node
- ADD : Multiple selection : Added correction referring to maximum parent-child relationship hierarchy number for handle operation
- ADD : Animation Window : Correspondence between Clamp, Mirror and Root Correction operation when operated on Animation Window side
- ADD : Animation Window : Correspondence of action to select Keyframe at current time when Animation Curve selection is synchronized
- FIX : Blend Shape : Copy and Paste problem
- FIX : Animator IK : Correction of calculation
- FIX : Original IK : Correction of calculation
- FIX : Root Correction : Correction for interpolation after the last frame
- FIX : speedup

Version 1.1.0p1
- FIX : MenuItem : Delete priority specification
- FIX : Fixed problem by UnityEditor language change
- FIX : RootCorrection : Update method update
- FIX : FootIK : Changes to force enabling in Timeline link state
- FIX : FootIK : Update method update
- FIX : Profiler relationship was left, so delete
- FIX : Unity2018.2 : Timeline

Version 1.1.0
- ADD : Unity2018.1 support
- ADD : Language select : Japanese
- ADD : Humanoid Root correction : Change from Lock button to Disable, Single and Full button. Full function addition
- FIX : Action correction when edited by AnimationWindow
- FIX : OriginalIK : Basic : Change the initial Weight of the tip to 0.5
- FIX : Selection Set : Set default name from active object
- FIX : Hierarchy : Change of Icon acquisition method
- FIX : Correction of reverse rotation correction processing
- FIX : FootIK : Update processing change
- FIX : BlendShape : Correspondence of the part where mirror correspondence was insufficient. Changed to be the value at the start of editing instead of 0 by reset
- FIX : Unity2018.2 : Error

Version 1.0.9
- ADD : Multiple selection behavior in PivotMode.Center
- ADD : Added warning when 'Based Upon' setting is not Original
- FIX : Improvement of multiple selection operation
- FIX : Action correction when edited by AnimationWindow
- FIX : speedup

Version 1.0.8
- ADD : Blend Pose
- FIX : Editing while paused : Fixed a bug that did not get correct animation and time on shortcut launch
- FIX : Editing while paused : Fixed a bug that sometimes did not return to the original position after editing
- FIX : Exporter : Texture output which is not Texture2D also supports texture output, error check added
- FIX : AnimatorIK : Head : Fixed bug where unintended initialization occurred
- FIX : Select Bone : Frontmost polygon may not be selected Bug fix
- FIX : speedup

Version 1.0.7p1
- FIX : AnimationWindow automatic lock at editing : Changed to invalid in Timeline
- FIX : Selection Set : Null error

Version 1.0.7
- ADD : Selection Set
- ADD : AnimationWindow automatic lock at editing
- ADD : Error display for forced termination of edit mode is added
- FIX : Tools : Clearnup : Correspondence of BlendShape information
- FIX : ControlWindow : Correction of Humanoid selection process

Version 1.0.6
- ADD : Edit BlendShape
- FIX : AnimatorIK : Head Swivel correspondence
- FIX : Fixed bug where Free Rotate Handle was not running
- FIX : EditorWindow : Add ToolBar, move previous element to Options
- FIX : MuscleGroup : Fixed a bug that was added in a situation where it is not necessary to add a curve with Reset
- FIX : IKTarget : Correction of range selection defect when space is not Global
- FIX : Humanoid : Fixed bug where Global rotation of Head got wrong when Neck does not exist
- FIX : Other bug fixes, speedup

Version 1.0.5
- ADD : Tools : Create New Clip
- ADD : Startup shortcut key correspondence
- FIX : Action correction when Animator is moved to a different hierarchy from Avatar creation time
- FIX : Correction of behavior that Glocal rotation operation does not reflect correctly
- FIX : Fixed a bug when IKTarget's Mirror reflected and each other's space was different
- FIX : DaeExporter : Correcting errors in the material without the '_Color' property

Version 1.0.4
- ADD : IK : Global, Local and Parent space switching supported
- ADD : IK : Automatic reflection switching of Rotation
- FIX : Fixed a bug where Mirror's curve change is not reflected in AnimationWindow
- FIX : Other bug fixes, speedup
- FIX : Documentation : Add a description
- FIX : Unity 2018.1 : Fix error

Version 1.0.3
- ADD : Original IK : Limb IK
- FIX : Original IK : GUI
- FIX : Hotkeys : Editor Window focus state
- FIX : Hotkeys : Change Keypad Plus and Minus

Version 1.0.2p2
- FIX : Timeline : Fixed problem that Dummy Object disappeared
- FIX : Timeline : Fix to Active change

Version 1.0.2p1
- ADD : Timeline : Dummy Timeline Position Type
- FIX : Timeline : Root : Reset All

Version 1.0.2
- ADD : Original IK
- ADD : Save Toolbar Valid State
- ADD : IK range selection
- ADD : Hierarchy : Selected Object Auto Expand Setting
- FIX : Hotkeys : Scene View focus state only
- FIX : Animator IK
- FIX : Settings : IK Default
- FIX : Reverse rotation correction processing

Version 1.0.1
- ADD : Legacy (Animation Component) support
- FIX : VA Tools : "Remove Save Settings" and "Replace Reference"

Version 1.0.0p1
- ADD : Generic Mirror condition setting, Ignore setting
- FIX : Save Settings

Version 1.0.0
- first release

