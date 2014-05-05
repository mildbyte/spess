spess
=====

a spess trading game

development is sort of suspended because Easter Term and exams, but I try to make at least one commit a day to not break the streak (eugh).

spess?
------

Inspired by [this paper](http://larc.unt.edu/techreports/LARC-2010-03.pdf), the X series, Mount & Blade and maybe something else.

The main idea behind this is to create a game with a fully simulated dynamic economy, sort of like the X series but with even more freedom and emergent&trade; gameplay&trade; The problem with X's economy was that the goods had a minimum and a maximum value, so buying something at its lowest value (like energy cells for 12 credits) was a sure way to a profit. I also liked the stock market in X3AP, but it had a similar problem, as well as the fact that the value of the companies in the stock market wasn't actually related to the state of the game.

The stock market, or the space exchange, is now where all the action happens. Owners of stations sell their goods and buy resources, speculators try to profit by moving goods between exchanges and traders buy and sell goods without ever touching or using them in an attempt to make money on the price movements. The exchange operates similarly to the real-world stock market with limit orders and order matching.

Of course, something has got to make the prices move, so I'm currently thinking about how owners would determine the price at which to sell/buy goods. The price movements have to be stable and at the same time worthwhile for the player to investigate and exploit. Another thing I want to maintain is that the money in the universe is conserved: it doesn't just appear (unless the government mints it) and disappear, as do the goods. Maybe throwing into space lots of AI agents that all try to make a profit will help, but there is also the problem of owners of stations determining how to reinvest their earnings: whether to buy extra ships or extra stations or invest into the stock exchange. What will make the price of the company shares on the stock exchange move?

There isn't much to do in spess for now. You need a version of MonoGame installed to compile and launch it, as well as some sort of a Visual Studio. When you start the game, you will be in a sector with a space exchange and a cabbage farm that lacks cabbage seeds and soil, so it will place an order for one at the exchange. A soil and seeds supplier ship is near the exchange, you can right click it and give it an order to deposit the goods and then sell them on the exchange, which will match with the station's order, so it will order a supplier ship to move the resources to it and a portion of delicious space cabbages will be grown.