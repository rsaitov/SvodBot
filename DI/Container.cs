using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using SvodBot.Bot;
using SvodBot.Executor;

namespace SvodBot.DI
{
    public class Container
    {
        public readonly IBot _bot;
        private readonly IExecutor _executor;
        public Container(IBot bot, IExecutor executor)
        {
            _bot = bot;
            _executor = executor;            
        }

        public void Execute()
        {

        }


    }
}