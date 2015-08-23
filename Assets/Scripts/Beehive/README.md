# Beehive - Behaviour Tree for C\#
[Beehive at GitHup](https://github.com/mfagerlund/Beehive)

Features:

 * Allows you to write scripted Behaviour Trees that can be edited and reloaded at run time.
 * Supports reading properties from your Agents (things controlled by the Behaviour Tree) using reflection
 * Supports special coroutines which makes some more straight forward.
 * Utility selectors, where a function describes the ordering of sub-tasks, instead of using the order in the script.
 * A Lisp like scripting language for computing values, used in Utility tasks and optional tasks.

For information on Behaviour Trees, see [Wikipedia: Behavior Trees (artificial intelligence, robotics and control)](https://en.wikipedia.org/wiki/Behavior_Trees_%28artificial_intelligence,_robotics_and_control%29).

I'm no good at writing sample / filler text, so go write something yourself.

Here's an example of a Behaviour Tree script, it's for a movie goer that;
* Runs to the exit if there's an alarm
* Goes to the bathroom if the need is great enough
* Watches the movie if everything is working out.

```
(BehaviourTree :RunningTaskHysterisis=10 
    (?
        (→  :if=IsAlarmOn /* Comments are allowed */     
            (→ 
                (GoToExit) 
                (Exit))
            (Panic :Message=""I can't find the exit!""))     
        (→  :if=(> BathroomNeed 0.8)
            (?
                (→ 
                    (GoToBathroom) 
                    (UseBathroom))
                (Panic :Message=""I can't find the bathroom!"")))
        (→
            (GoToCinema)
            (ReserveSeat)
            (GoToSelectedTile)
            (RunOnce 
              :Name=""Here's a name for you""
              (DebugLog (Format ""This seems {0} to be working {1}"" 1 5.5)))
            (StayInCenterOfSelectedTile))))
```

# Supported Behaviour Tree Methods
## Control Flow
### Sequence (→)
### Selector (?)
## Utility Control Flow
### UtilitySequence 
### UtilitySelector 
## Decorators
### Counter
### AlwaysFail
### AlwaysSucceed
### Invert
### UntilFail
### UntilSuccess
### RunOnce
### RunNTimes
## Leaves
### Fail
### Succeed
### KeepRunning



## LD
I'm releasing this code because I hope to use it to create my AI in upcoming Ludum Dares, and I invite you to try it out too!
