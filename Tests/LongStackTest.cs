using Microsoft.VisualStudio.TestTools.UnitTesting;
using CribbageSolitaireSolver;

namespace Tests
{
    [TestClass]
    public class LongStackTest
    {
        [TestMethod]
        public void Count_Empty()
        {
            ulong myStack = 0;
            byte count = LongStack.Count(myStack);
            Assert.AreEqual(0, count);
        }

        [TestMethod]
        public void Peek_what_I_pushed()
        {
            ulong myStack = 0;
            myStack = LongStack.Push(myStack, 12);
            Assert.AreEqual(12, LongStack.Peek(myStack));
        }

        [TestMethod]
        public void Push_pop_count()
        {
            ulong myStack = 0;
            
            myStack = LongStack.Push(myStack, 12);
            Assert.AreEqual(1, LongStack.Count(myStack));

            myStack = LongStack.Push(myStack, 3);
            Assert.AreEqual(2, LongStack.Count(myStack));

            myStack = LongStack.Pop(myStack);
            Assert.AreEqual(1, LongStack.Count(myStack));

            myStack = LongStack.Pop(myStack);
            Assert.AreEqual(0, LongStack.Count(myStack));
        }

        [TestMethod]
        public void ElementAt()
        {
            ulong myStack = 0;

            myStack = LongStack.Push(myStack, 11);
            myStack = LongStack.Push(myStack, 12);
            myStack = LongStack.Push(myStack, 13);
            myStack = LongStack.Push(myStack, 4);

            Assert.AreEqual(4, LongStack.ElementAt(myStack, 0));
            Assert.AreEqual(13, LongStack.ElementAt(myStack, 1));
            Assert.AreEqual(12, LongStack.ElementAt(myStack, 2));
            Assert.AreEqual(11, LongStack.ElementAt(myStack, 3));

            myStack = LongStack.Pop(myStack);
            myStack = LongStack.Pop(myStack);

            Assert.AreEqual(12, LongStack.ElementAt(myStack, 0));
            Assert.AreEqual(11, LongStack.ElementAt(myStack, 1));
        }
    }
}
