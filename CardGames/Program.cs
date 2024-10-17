// void print(Object obj) { Console.WriteLine(obj); }

Durak game = new Durak(1);
await game.Play();

// async Task<string> asyncReadLine() {
//     return await Task.Run(() => Console.ReadLine() ?? string.Empty);
// }

// async Task asyncPrint() {
//     string msg = await asyncReadLine();
//     Console.WriteLine($"Congratulations! You entered: {msg}");
// }

// async Task waiting() {
//     int counter = 0;
//     for (int i = 0; i < 10; i++) {
//         Console.WriteLine("waiting " + counter++);
//         await Task.Delay(1000);
//     }
// }

// Task task1 = asyncPrint();
// Task task2 = waiting();

// await Task.WhenAll(task1, task2);
