using System;
using System.Collections.Generic;

namespace Clones
{
    class OneWayLinkedStack<T>
    {
        class StackItem
        {
            public T Value { get; set; }
            public StackItem Next { get; set; }
        }

        public OneWayLinkedStack<T> Copy()
        {
            return new OneWayLinkedStack<T>() { head = this.head };
        }

        private StackItem head;
        public bool IsEmpty => head == null;

        public void Push(T value)
        {
            head = new StackItem() { Value = value, Next = head };
        }

        public T Pop()
        {
            if (IsEmpty)
                throw new InvalidOperationException("OneWayLinkedStack is empty");
            T value = head.Value;
            head = head.Next;
            return value;
        }

        public T Peek()
        {
            if(IsEmpty)
                throw new InvalidOperationException("OneWayLinkedStack is empty");
            return head.Value;
        }

        public void Clear() => head = null;
    }

	public class CloneVersionSystem : ICloneVersionSystem
	{
        private List<Clone> clones = new List<Clone>() { new Clone() };

		public string Execute(string query)
		{
            string[] words = query.Split();
            string command = words[0];
            List<int> args = new List<int>();
            for (int i = 1; i < words.Length; i++)
            {
                args.Add(int.Parse(words[i]));
            }

            if (args.Count >= 1 && clones.Count >= args[0])
            {
                int cloneIndex = args[0] - 1;
                switch (command)
                {
                    case "learn":
                        if (args.Count >= 2)
                        {
                            new LearnCommand(args[1]).Execute(clones[cloneIndex]);
                        }
                        break;
                    case "rollback":
                        new RollbackCommand().Execute(clones[cloneIndex]);
                        break;
                    case "relearn":
                        new RelearnCommand().Execute(clones[cloneIndex]);
                        break;
                    case "clone":
                        clones.Add(clones[cloneIndex].Copy());
                        break;
                    case "check":
                        return clones[cloneIndex].LastProgram;
                }
            }
            return null;
		}
	}

    class Clone 
    {
        public OneWayLinkedStack<int> ProgramsLearnedOrder { get; set; } = new OneWayLinkedStack<int>();
        public OneWayLinkedStack<LearnCommand> LearnCommandHistory { get; set; } = new OneWayLinkedStack<LearnCommand>();
        public OneWayLinkedStack<RollbackCommand> RollbackCommandHistory { get; set; } = new OneWayLinkedStack<RollbackCommand>();

        public string LastProgram
        {
            get
            {
                if (ProgramsLearnedOrder.IsEmpty)
                    return "basic";
                return ProgramsLearnedOrder.Peek().ToString();
            }
        }

        public Clone Copy()
        {
            return new Clone()
            {
                ProgramsLearnedOrder = this.ProgramsLearnedOrder.Copy(),
                LearnCommandHistory = this.LearnCommandHistory.Copy(),
                RollbackCommandHistory = this.RollbackCommandHistory.Copy()
            };
        }
    }

    class LearnCommand
    {
        private int programIndex;

        public LearnCommand(int programIndex) => this.programIndex = programIndex;

        public void Execute(Clone receiver, bool initialExecution = true)
        {
            receiver.ProgramsLearnedOrder.Push(programIndex);
            receiver.LearnCommandHistory.Push(this);
            if (initialExecution)
                receiver.RollbackCommandHistory.Clear();
            
        }

        public LearnCommand Undo(Clone receiver)
        {
            receiver.ProgramsLearnedOrder.Pop();
            return this;
        }
    }

    class RollbackCommand
    {
        private LearnCommand rollbackedCommand;

        public void Execute(Clone receiver)
        {
            if (!receiver.LearnCommandHistory.IsEmpty)
            {
                rollbackedCommand = receiver.LearnCommandHistory.Pop().Undo(receiver);
                receiver.RollbackCommandHistory.Push(this);
            }
        }

        public void Undo(Clone receiver)
        {
            rollbackedCommand.Execute(receiver, false);
        }
    }

    class RelearnCommand
    {
        public void Execute(Clone receiver)
        {
            if (!receiver.RollbackCommandHistory.IsEmpty)
                receiver.RollbackCommandHistory.Pop().Undo(receiver);
        }
    }
}