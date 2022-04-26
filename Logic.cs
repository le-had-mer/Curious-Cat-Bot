using DSharpPlus.CommandsNext;
using DSharpPlus.Entities;
using DSharpPlus.Interactivity.Extensions;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace Curious_Cat

{
    public static class Rules
    {
        public const int Delay = 3000;

        public const int Actions = 5;

        public const double FoodDelta = 0.01;
        public const double ClawsDelta = 0.06;
        public const double PeeDelta = 0.085;
        public const double SleepDelta = 0.045;

        public const double FoodUse = -0.3;
        public const double ClawsUse = -0.6;
        public const double PeeUse = -0.9;
        public const double SleepUse = -0.1;

        public const int Places = 4;

        public const string PetBowl = "миска";
        public const string ScratchingPost = "когтеточка";
        public const string PetТray = "лоток";
        public const string PetHouse = "домик";
    }

    public enum UserReaction
    {
        Idle = 0,
        GoodBoy = 1,
        BadBoy = 2
    }

    public class Memory : List<Flashback>
    {
        double[,] table;

        public double GetReaction(int a, int p)
        {
            return table[a, p];
        }

        public double GetSum(Memory memory)
        {
            double sum = 0;
            foreach (var item in this)
            {
                sum += memory.GetReaction(item.actionNumber, item.placeNumber);
            }
            return sum;
        }

        public void Forget(int day)
        {
            List<Flashback> delete = new List<Flashback>();
            foreach (var item in this)
            {
                item.timeSpan++;
                if (item.timeSpan >= day)
                {
                    delete.Add(item);
                }
            }
            foreach (var item in delete)
            {
                this.Remove(item);
            }
        }

        public void FillTablePoor(Memory memory)
        {
            table = new double[Rules.Actions - 1, Rules.Places];
            for (int i = 0; i < Rules.Actions - 1; i++)
            {
                for (int j = 0; j < Rules.Places; j++)
                {
                    List<Double> sum = new List<double>();
                    foreach (var wish in this)
                    {
                        if ((wish.placeNumber == i) & (wish.placeNumber == j)) sum.Add(memory.GetReaction(i, j));
                    }
                    double s = 0;
                    foreach (double d in sum)
                    {
                        s += d;
                    }

                    if (sum.Count != 0) table[i, j] = s / sum.Count;
                    else table[i, j] = 0;
                }
            }
        }

        public void FillTable()
        {
            table = new double[Rules.Actions - 1, Rules.Places];
            for (int i = 0; i < Rules.Actions - 1; i++)
            {
                for (int j = 0; j < Rules.Places; j++)
                {
                    List<Double> sum = new List<double>();
                    foreach (var wish in this)
                    {
                        if ((wish.actionNumber == i) & (wish.placeNumber == j + Rules.Actions))
                        {
                            if (wish.reaction == UserReaction.GoodBoy)
                            {
                                sum.Add(1);
                            }
                            else if (wish.reaction == UserReaction.BadBoy)
                            {
                                sum.Add(-1);
                            }
                            else if (wish.reaction == UserReaction.Idle)
                            {
                                sum.Add(0);
                            }
                        }
                    }
                    double s = 0;
                    foreach (double d in sum)
                    {
                        s += d;
                    }

                    if (s != 0) table[i, j] = s / sum.Count;
                    else table[i, j] = 0;
                }

                double max = table[i, 0];
                for (int j = 0; j < Rules.Places; j++)
                { 
                    if (table[i, j] > max)
                    {
                        max = table[i, j];
                    }
                }
                if (max == 0)
                    for (int j = 0; j < Rules.Places; j++)
                    {
                        if (table[i, j] < max)
                        {
                            max = table[i, j];
                        }
                    }
                if (max == 0)
                {
                    for (int j = 0; j < Rules.Places; j++)
                    {
                        table[i, j] = 1;
                    }
                }
                else 
                {
                    for (int j = 0; j < Rules.Places; j++)
                    {
                        if (table[i, j] == 0)
                        {
                            table[i, j] = -max;
                        }
                    }
                }
            }
        }
    }

    public class Flashback
    {
        public int actionNumber;
        public int placeNumber;
        public UserReaction reaction;

        public int timeSpan;

        public Flashback(int actionNumber, int placeNumber)
        {
            this.actionNumber = actionNumber;
            this.placeNumber = placeNumber;
        }


        public Flashback(int actionNumber, int placeNumber, UserReaction reaction)
        {
            this.actionNumber = actionNumber;
            this.placeNumber = placeNumber;
            this.reaction = reaction;

            this.timeSpan = 1;
        }
    }

    public class Entity
    {
        public NeuralNetwork mind;

        public double food, claws, pee, sleep;

        public double score;
        public double score2;
        public double score3;

        public Memory memory;

        public void SetNeeds(double food, double claws, double pee, double sleep)
        {
            this.food = food;
            this.claws = claws;
            this.pee = pee;
            this.sleep = sleep;

            
        }

        public Entity(NeuralNetwork network)
        {
            mind = network;
            score = 0;
            score2 = 0;
            score3 = 0;
        }

    }



    class Logic
    {
        double food, claws, pee, sleep;

        public List<Entity> currentGeneration;

        public Logic(int amount, params int[] layers)
        {
            currentGeneration = new List<Entity>();

            for (int i = 0; i < amount; i++)
            {
                currentGeneration.Add(new Entity(new NeuralNetwork(NeuralNetwork.WeightType.Random, layers)));
            }
            food = claws = pee = sleep = 0;
        }

        public NeuralNetwork RunSchedulePrimitive(int count, double quality)
        {
            for (int u = 0; u < count; u++)
            {
                Random rnd1 = new Random();
                double genFood = rnd1.NextDouble();
                double genClaws = rnd1.NextDouble();
                double genPee = rnd1.NextDouble();
                double genSleep = rnd1.NextDouble();

                for (int e = 0; e < currentGeneration.Count; e++)
                {
                    Entity next = currentGeneration[e];
                    food = genFood;
                    claws = genClaws;
                    pee = genPee;
                    sleep = genSleep;

                    while (true)
                    {
                        double fit = next.score;
                        Random rnd = new Random();

                        double[] input = { food, claws, pee, sleep };
                        double[] output = next.mind.RunIteration(input);

                        if (next.score > 500)
                        {
                            Console.WriteLine($"WOWOW {next.score}");
                        }

                        int max = rnd.Next(0, 5);
                        for (int i = 0; i < 5; i++)
                        {
                            if (output[i] > output[max])
                            {
                                max = i;
                            }
                            else if (output[i] == output[max])
                            {
                                if (rnd.Next(0, 2) == 0) max = i;
                            }
                        }
                        switch (max)
                        {
                            case 0:
                                food -= 0.3;
                                if (food < 0)
                                {
                                    next.score += food;
                                    next.score3 += 1;
                                    food = 0;
                                }
                                else if (food > 1)
                                {
                                    next.score += 1;
                                    next.score3 += 1;
                                    food = 1 + Rules.FoodUse;
                                }
                                else
                                {
                                    next.score += 1;
                                    next.score2 += 1;
                                }
                                break;
                            case 1:
                                claws -= 0.6;
                                if (claws < 0)
                                {
                                    next.score += claws;
                                    next.score3 += 1;
                                    claws = 0;
                                }
                                else if (claws > 1)
                                {
                                    next.score += 1;
                                    next.score3 += 1;
                                    claws = 1 + Rules.ClawsUse;
                                }
                                else
                                {
                                    next.score += 1;
                                    next.score2 += 1;
                                }
                                break;
                            case 2:
                                pee -= 0.9;
                                if (pee < 0)
                                {
                                    next.score += pee;
                                    next.score3 += 1;
                                    pee = 0;
                                }
                                else if (pee > 1)
                                {
                                    next.score += 1;
                                    next.score3 += 1;
                                    pee = 1 + Rules.PeeUse;
                                }
                                else
                                {
                                    next.score += 1;
                                    next.score2 += 1;
                                }
                                break;
                            case 3:
                                sleep -= 0.1;
                                if (sleep < 0)
                                {
                                    next.score += sleep;
                                    next.score3 += 1;
                                    sleep = 0;
                                }
                                else if (sleep > 1)
                                {
                                    next.score += 1;
                                    next.score3 += 1;
                                    sleep = 1 + Rules.SleepUse;
                                }
                                else
                                {
                                    next.score += 1;
                                    next.score2 += 1;
                                }
                                break;
                        }

                        if (fit > next.score) break;
                        fit = next.score;


                        food += 0.01;
                        claws += 0.006;
                        pee += 0.0085;
                        sleep += 0.0045;

                        if (food > 1)
                        {
                            next.score -= food - 1;
                        }
                        if (claws > 1)
                        {
                            next.score -= claws - 1;
                        }
                        if (pee > 1)
                        {
                            next.score -= pee - 1;
                        }
                        if (sleep > 1)
                        {
                            next.score -= sleep - 1;
                        }

                        if (fit > next.score) break;

                        if (next.score >= 10000)
                        {
                            break;
                        }
                    }
                }

                Entity best = currentGeneration[0];
                Entity second = currentGeneration[0];

                foreach (var item in currentGeneration)
                {
                    if (item.score > best.score)
                    {
                        second = best;
                        best = item;
                    }
                }

                if (best.score >= quality)
                {
                    return best.mind;
                }

                Console.WriteLine($"t {best.score} a {best.score2} na {best.score3}");

                List<Entity> newGeneration = new List<Entity>();
                NeuralNetwork[] childs;

                for (int i = 0; i < 20; i++)
                {
                    childs = NeuralNetwork.Crossing(best.mind, second.mind, 2);

                    foreach (var item in childs)
                    {
                        newGeneration.Add(new Entity(item));
                        newGeneration.Add(new Entity(NeuralNetwork.Mutation(item, 1)));
                        newGeneration.Add(new Entity(NeuralNetwork.Mutation(item, 2)));
                        newGeneration.Add(new Entity(NeuralNetwork.Mutation(item, 3)));

                        newGeneration.Add(new Entity(best.mind));
                    }
                }
                currentGeneration = newGeneration;
            }
            return null;
        }

    }

    class LogicSec
    {
        double food, claws, pee, sleep;
        public Entity currentGeneration;
        const int actionsLimit = 10;

        CommandContext sessionContext;

        int outputs;

        Memory memory;

        public LogicSec(CommandContext ctx, NeuralNetwork initNetwork)
        {
            Random rnd = new Random();

            outputs = initNetwork.layers[initNetwork.layers.Count - 1].Count;

            sessionContext = ctx;
            currentGeneration = new Entity(initNetwork);

            food = rnd.NextDouble();
            claws = rnd.NextDouble();
            pee = rnd.NextDouble();
            sleep = rnd.NextDouble();

            memory = new Memory();
        }

        public void SleepNLearn()
        {
            List<Entity> dreamGeneration = new List<Entity>();
            //dreamGeneration.Add(currentGeneration);

            Entity best;
            best = currentGeneration;

            double genFood = currentGeneration.food;//rnd1.NextDouble();
            double genClaws = currentGeneration.claws;//rnd1.NextDouble();
            double genPee = currentGeneration.pee;//rnd1.NextDouble();
            double genSleep = currentGeneration.sleep;//rnd1.NextDouble();

            for (int g = 0; g < 999; g++)
            {
                dreamGeneration.Clear();
                for (int f = 0; f < 10; f++)
                {
                    dreamGeneration.Add(new Entity(NeuralNetwork.Mutation(best.mind, 2)));
                    dreamGeneration.Add(new Entity(NeuralNetwork.Mutation(best.mind, 4)));
                    dreamGeneration.Add(new Entity(NeuralNetwork.Mutation(best.mind, 6)));
                    dreamGeneration.Add(new Entity(NeuralNetwork.Mutation(best.mind, 8)));
                    dreamGeneration.Add(new Entity(NeuralNetwork.Mutation(best.mind, 10)));
                    dreamGeneration.Add(new Entity(NeuralNetwork.Mutation(best.mind, 12)));
                    dreamGeneration.Add(new Entity(NeuralNetwork.Mutation(best.mind, 14)));
                    dreamGeneration.Add(new Entity(NeuralNetwork.Mutation(best.mind, 16)));
                    dreamGeneration.Add(new Entity(NeuralNetwork.Mutation(best.mind, 18)));
                    dreamGeneration.Add(new Entity(NeuralNetwork.Mutation(best.mind, 20)));
                }
              
                foreach (Entity cat in dreamGeneration)
                {
                    cat.memory = new Memory();

                    cat.SetNeeds(genFood, genClaws, genPee, genSleep);

                    while (true)
                    {
                        double fit = cat.score;
                        Random rnd = new Random();

                        double[] input = { cat.food, cat.claws, cat.pee, cat.sleep };
                        double[] output = cat.mind.RunIteration(input);

                        int maxNeed = rnd.Next(0, Rules.Actions);
                        for (int i = 0; i < Rules.Actions; i++)
                        {
                            if (output[i] > output[maxNeed])
                            {
                                maxNeed = i;
                            }
                            else if (output[i] == output[maxNeed])
                            {
                                if (rnd.Next(0, 2) == 0) maxNeed = i;
                            }
                        }

                        int maxPlace = rnd.Next(Rules.Actions, Rules.Places + Rules.Actions);
                        for (int i = Rules.Actions; i < Rules.Actions + Rules.Places; i++)
                        {
                            if (output[i] > output[maxPlace])
                            {
                                maxPlace = i;
                            }
                            else if (output[i] == output[maxPlace])
                            {
                                if (rnd.Next(0, 2) == 0) maxPlace = i;
                            }
                        }

                        if (maxNeed != Rules.Actions - 1)
                        {
                            cat.score2 += memory.GetReaction(maxNeed, maxPlace - Rules.Actions);
                            cat.memory.Add(new Flashback(maxNeed, maxPlace - Rules.Actions));
                        }
                        switch (maxNeed)
                        {
                            case 0:
                                cat.food += Rules.FoodUse;
                                if (cat.food < 0)
                                {
                                    cat.score += cat.food;
                                    cat.food = 0;
                                }
                                else if (cat.food > 1)
                                {
                                    cat.score += 1;
                                    cat.food = 1 + Rules.FoodUse;
                                }
                                else
                                {
                                    cat.score += 1;
                                }
                                break;
                            case 1:
                                cat.claws += Rules.ClawsUse;
                                if (cat.claws < 0)
                                {
                                    cat.score += cat.claws;
                                    cat.claws = 0;
                                }
                                else if (cat.claws > 1)
                                {
                                    cat.score += 1;
                                    cat.claws = 1 + Rules.ClawsUse;
                                }
                                else
                                {
                                    cat.score += 1;
                                }
                                break;
                            case 2:
                                cat.pee += Rules.PeeDelta;
                                if (cat.pee < 0)
                                {
                                    cat.score += cat.pee;
                                    cat.pee = 0;
                                }
                                else if (cat.pee > 1)
                                {
                                    cat.score += 1;
                                    cat.pee = 1 + Rules.PeeUse;
                                }
                                else
                                {
                                    cat.score += 1;
                                }
                                break;
                            case 3:
                                cat.sleep += Rules.SleepUse;
                                if (cat.sleep < 0)
                                {
                                    cat.score += cat.sleep;
                                    cat.sleep = 0;
                                }
                                else if (cat.sleep > 1)
                                {
                                    cat.score += 1;
                                    cat.sleep = 1 + Rules.SleepUse;
                                }
                                else
                                {
                                    cat.score += 1;
                                }
                                break;
                        }

                        if ((fit > cat.score) | (cat.score > 2000)) break;
                        else fit = cat.score;

                        cat.food += Rules.FoodDelta;
                        cat.claws += Rules.ClawsDelta;
                        cat.pee += Rules.PeeDelta;
                        cat.sleep += Rules.SleepDelta;

                        if (cat.food > 1)
                        {
                            cat.score -= cat.food - 1;
                        }
                        if (cat.claws > 1)
                        {
                            cat.score -= cat.claws - 1;
                        }
                        if (cat.pee > 1)
                        {
                            cat.score -= cat.pee - 1;
                        }
                        if (cat.sleep > 1)
                        {
                            cat.score -= cat.sleep - 1;
                        }
                        if (fit > cat.score) break;
                    }

                    cat.memory.FillTablePoor(memory);

                    cat.score3 = cat.memory.GetSum(memory);
                }                              

                //Entity best = dreamGeneration[0];

                foreach (var item in dreamGeneration)
                {                    
                    if (item.score3 > best.score3)
                    {
                        Console.WriteLine("Fuck Yeah!");
                        best = item;                        
                    }
                }
                Console.WriteLine($"score {best.score} delta {best.score3} count {best.memory.Count}");

                if (best.score3 > 2000)
                {
                    break;
                }

                //currentGeneration = best;

            }
            sessionContext.RespondAsync("Новый день, новые ошибки");
            currentGeneration.mind = best.mind;
            RunRealtime();
        }

        async public void/*NeuralNetwork*/ RunRealtime()
        {
            memory.Forget(5);
            Random rnd = new Random();
            int co = 0;
            int actions = 0;
            do
            {
                await Task.Delay(Rules.Delay);
                double[] inputs = { food, claws, pee, sleep };

                Entity currentEntity = currentGeneration;
                double[] output = currentEntity.mind.RunIteration(inputs);

                int maxNeed = rnd.Next(0, Rules.Actions);
                for (int i = 0; i < Rules.Actions; i++)
                {
                    if (output[i] > output[maxNeed])
                    {
                        maxNeed = i;
                    }
                    else if (output[i] == output[maxNeed])
                    {
                        if (rnd.Next(0, 2) == 0) maxNeed = i;
                    }
                }

                int maxPlace = rnd.Next(Rules.Actions, Rules.Places + Rules.Actions);
                for (int i = Rules.Actions; i < Rules.Actions + Rules.Places; i++)
                {
                    if (output[i] > output[maxPlace])
                    {
                        maxPlace = i;
                    }
                    else if (output[i] == output[maxPlace])
                    {
                        if (rnd.Next(0, 2) == 0) maxPlace = i;
                    }
                }
                                                
                string response = "";
                response += co + ". ";

                if (maxNeed < 4)
                {
                    switch (maxNeed)
                    {
                        case 0:
                            food += Rules.FoodUse;
                            if (food < 0)
                            {
                                food = 0;
                                response += "[питомец переел, утоляя голод с помощью ";
                            }
                            else if (food > 1)
                            {
                                food = 1 + Rules.FoodUse;
                                response += "[питомец наконец покушал, утоляя голод с помощью ";
                            }
                            else
                            {
                                response += "[питомец покушал, утоляя голод с помощью ";
                            }
                            actions += 1;
                            break;
                        case 1:
                            claws += Rules.ClawsUse;
                            if (claws < 0)
                            {
                                claws = 0;
                                response += "[питомец сильно сточил когти об ";
                            }
                            else if (claws > 1)
                            {
                                claws = 1 + Rules.ClawsUse;
                                response += "[питомец наконец подточил когти об ";
                            }
                            else
                            {
                                response += "[питомец подточил когти об ";
                            }
                            actions += 1;
                            break;
                        case 2:
                            pee += Rules.PeeUse;
                            if (pee < 0)
                            {
                                pee = 0;
                                response += "[питомец слегка обмочился на ";
                            }
                            else if (pee > 1)
                            {
                                pee = 1 + Rules.PeeUse;
                                response += "[питомец наконец помочился на ";
                            }
                            else
                            {
                                response += "[питомец помочился на ";
                            }
                            actions += 1;
                            break;
                        case 3:
                            sleep += Rules.SleepUse;
                            if (sleep < 0)
                            {
                                sleep = 0;
                                response += "[питомец немного вздремнул на ";
                            }
                            else if (sleep > 1)
                            {
                                sleep = 1 + Rules.SleepUse;
                                response += "[питомец наконец подремал на ";
                            }
                            else
                            {
                                response += "[питомец подремал на ";
                            }
                            actions += 1;
                            break;
                    }

                    if (maxPlace == 5)
                    {
                        response += Rules.PetBowl;
                    }
                    else if (maxPlace == 6)
                    {
                        response += Rules.ScratchingPost;
                    }
                    else if (maxPlace == 7)
                    {
                        response += Rules.PetТray;
                    }
                    else if (maxPlace == 8)
                    {
                        response += Rules.PetHouse;
                    }

                    response += "]\n";
                }

                food += Rules.FoodDelta;
                claws += Rules.ClawsDelta;
                pee += Rules.PeeDelta;
                sleep += Rules.SleepDelta;

                if (food > 1)
                {
                    response += "[питомец голодает]\n";

                }
                if (claws > 1)
                {
                    response += "[у питомца когти чешутся]\n";
                }
                if (pee > 1)
                {
                    response += "[питомец еле сдерживает позыв]\n";
                }
                if (sleep > 1)
                {
                    response += "[питомец вялый]\n";
                }
                
                response+= $"голод {Math.Round(food, 3)}; когти {Math.Round(claws, 3)}; нужда {Math.Round(pee, 3)}; сон {Math.Round(sleep, 3)}";
                var a = await sessionContext.RespondAsync(response);

                if (maxNeed != 4)
                {
                    await a.CreateReactionAsync(DiscordEmoji.FromName(sessionContext.Client, ":thumbsdown:"));
                    await a.CreateReactionAsync(DiscordEmoji.FromName(sessionContext.Client, ":thumbsup:"));

                    var interactivity = sessionContext.Client.GetInteractivity();

                    DateTime flash = DateTime.Now;

                    var b = await interactivity.WaitForReactionAsync(re =>
                    re.Message == a
                    && re.User != sessionContext.Client.CurrentUser
                    && ((re.Emoji == DiscordEmoji.FromName(sessionContext.Client, ":thumbsdown:"))
                    | (re.Emoji == DiscordEmoji.FromName(sessionContext.Client, ":thumbsup:")))
                    , TimeSpan.FromSeconds(50));

                    DateTime bang = DateTime.Now;
                    TimeSpan ts = bang - flash;

                    if (!b.TimedOut)
                    {
                        await sessionContext.TriggerTypingAsync();
                        if (b.Result.Emoji == DiscordEmoji.FromName(sessionContext.Client, ":thumbsdown:"))
                        {
                            await sessionContext.RespondAsync($"[:thumbsdown:] {ts.TotalSeconds}");
                            await a.DeleteAllReactionsAsync();
                            memory.Add(new Flashback(maxNeed, maxPlace, UserReaction.BadBoy));
                        }
                        else if (b.Result.Emoji == DiscordEmoji.FromName(sessionContext.Client, ":thumbsup:"))
                        {
                            await sessionContext.RespondAsync($"[:thumbsup:] {ts.TotalSeconds}");
                            await a.DeleteAllReactionsAsync();
                            memory.Add(new Flashback(maxNeed, maxPlace, UserReaction.GoodBoy));
                        }
                    }
                    else
                    {
                        await sessionContext.RespondAsync($"[:cat:] {ts.TotalSeconds}");
                        await a.DeleteAllReactionsAsync();
                        memory.Add(new Flashback(maxNeed, maxPlace, UserReaction.Idle));
                    }
                }                
                co++;
            } while (actions < actionsLimit);
            await sessionContext.RespondAsync("Питомец решил поспать и обдумать содеянное");
            memory.FillTable();

            SleepNLearn();
        }
    }
}
