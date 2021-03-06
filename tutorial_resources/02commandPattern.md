# Command Pattern

## Introduction

Let's say we have an interface or base class of some Item. Potion, scroll or bomb are children of that class. All of them have something in common: they can be **used**. Potion heals the player, scroll turns player into a cat, bomb makes an explosion. We have our inventory of these items and evertime we click Z,X or C we use them and the game will just execute an interface method ``` Item.use() ```. This is a simple and good solution, but in Object Oriented Programming we can do more than that.

There are some bugs in our game, so we have to log in file that we executed our ```Item.use()``` everytime we use any of our items. Moreover we want to add a feature to our game: replays. In our simple solution there would be a problem in doing this. What if we wanted to add special abilities? Jump mechanics? Interactable objects? Of course it would be doable, but there is a better way to execute a method of any object in our game. We can **encapsulate** triggers for that methods into objects which can be parametrized as we wish.

In the simplest form of the Command Design Pattern we have interfaces such as:

- ICommand - with method ```execute()``` 
- IExecutor - with method ```executeCommand(Command)```

Let's see a different example: 
```
[Serializable]
public class DestroyTileCommand : ICommand{
 Tile tile;
 TypeOfDestruction type; //Explosion? Mining? Teleporting into void? We need to know so we have to play correct animation and sound
 
 public DestroyTileCommand(Tile tile, TypeOfDestruction type)
 {
  this.tile = tile;
  this.type = type;
 }

public void Execute(GameData gameData)
 {
  gameData.Map.GetTile(tile.id).Destroy(type);
 }

}
```

Executor object would just take an Command that inheirits from ICommand and will execute them. However there are many ways we can execute it. If this is a client, we have to send this ICommand to the server so the server can execute it first and since our method is encapsulated into an object it can be easily send via network!
If we are testing something we can make our Executor object to save every Command so we can track our game and all changes in GameData to find bugs. What about replay? We just save all Commands in an Array and their timestamp and reexecute them from the start.

**Notice that we can't send a reference via network and we have to map item (for example: by id) so we can make sure that we are talking about the same item in the GameData on different computers!**
 
Command Design Pattern is a huge and powerful tool in structuring your code especially in networking as Commands can be serialized and send to other computers. You can modify your command with ```Undo()``` that reverts changes to the GameData. Do you now know how CTRL+Z is implemented in programs like Photoshop? 

What is crucial in here: **don't make your Executions too complex.** All logic should be inside Game Data objects. We just need commands to do some atomic operations that will be sent using network.

## Creating Commands and Views

As simple it sounds, our commands would be generated by MonoBehaviours: GUI objects or Input Handlers and then they will be sent to our Executors.

In previous tutorial it was said that we should split Game Data from Game Logic. Same should apply to the presentation of this Game Data. Whenever our commands makes any change in the Game Data, we can update our **Views** - GUI or world presentation objects to display new data. There is no point in reupdating our health bar each frame if we haven't made any changes in Game Data. This is strictly connected to other design pattern called **Observer**.

## Executor, Client, Server

IExecutors have one job: execute a command. They should behave differently if this were a multiplayer game. In last tutorial we said that in Client-Server architecture all operations(**commands**) executed locally should be sent to the server and it would confirm those. Remember that we should minimize possibility of our local Game Data to differ from server's Game Data. Incoherent data breaks an illusion of the multiplayer game. So IExecutors in our Client-Server architecture should look something like this:

```
public interface IExecutor 
{
 void Execute(ICommand command);
}

public class ServerExecutor : IExecutor 
{
.
.
.
 void Execute(ICommand command) //This fires up when we get any command from any of our clients
 {
  command.Execute(this.gameState);
  this.server.SendToAll(command); //Resending commands
 }
}

public class ClientExecutor : IExecutor {
 .
 .
 .
 
 void Execute(ICommand command) //This fires up when we do something locally
 {
  this.client.Send(command);
 }
 
 void UpdateGameState(ICommand command) //This fires up when we get command from the server
 {
  command.Execute(this.gameState)
 }
}

```

## Bigger picture!

To make it more clear, a simple example how this will work in a multiplayer game.

1) I press button that executes function that sends my command into my one IExecutor.
2) I execute Command C on my *ClientExecutor* CE that sends data to the server.
3) Server receives Command C as serialized JSON.
4) It has all the data needed to execute on *ServerExecutor* - executes it locally and sends Command C to all players.
5) We handle our data in such a way that we have our *ClientExecutor* and additional method that executes a Command but this time: locally without sending it to the server.
6) Our GameData has changed. Update our game: play a sound, change UI or do something else.

## Serializing Commands

JSON serialization makes a string from an object's fields. This string now can be used to create a new object of this type that will have exactly same field values. 
Simple example:
```
 string json = JsonUtility.ToJson(myCommandA);
 myCommandB = JsonUtility.FromJson<myCommandType>(myCommandA);
 //myCommandA and myCommandB have all fields equal
```
This can be used in our multiplayer as all operations(**commands**) can be turned into string that will be sent via network.

## Deserializing Commands
The problem is in deserializing those operations. We don't know their type as serializing doesn't serialize field: "name of the class" and we have to know what type of command we just got. This is my solution:
```
[Serializable]
public class SerializableClass
{
    [SerializeField]
    private string serializedClassName;

    public SerializableClass()
    {
        serializedClassName = this.GetType().ToString();
    }
}
```
And make sure that our particular ICommands also inheirit from SerializableClass in some way.
Then in our Command deserializer add something like this:
```
public static ICommand StringToCommand(string msg)
{
    SerializableClass ctype = JsonUtility.FromJson<SerializableClass>(msg);
    Type t = Type.GetType(ctype.GetClassName());
    ICommand gc = (ICommand)JsonUtility.FromJson(msg, t);
    return gc;
}
```
This would return a command whose type and fields' values are equal to the command sent.

## What's next?

In next tutorial we will see how one would go about implementing their own Server and Client class. This won't be a full code of doing it... but we will consider what those classes should do in our networking that uses Command Pattern. Especially if we want to make them asynchronous.

[Next ->](03clientserver.md)
