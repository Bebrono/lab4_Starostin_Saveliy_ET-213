using System;
using System.Collections.Generic;
using System.Linq;

namespace lab4_Starostin_Saveliy_ET_213 {
    class Program {
        class Product {
            public string Name { get; set; }
            public string Category { get; set; }
            public decimal Price { get; set; }
            public string Description { get; set; }
            public int Stock { get; set; }

            public Product(string name, string category, decimal price, string description, int stock) {
                Name = name;
                Category = category;
                Price = price;
                Description = description;
                Stock = stock;
            }

            public override string ToString() {
                return $"{Name} ({Category}) - {Price} руб\nОписание: {Description}\nНа складе: {Stock} шт.";
            }
        }

        class Cart {
            private Dictionary<Product, int> items = new Dictionary<Product, int>();

            public void AddProduct(Product product, int quantity = 1) {
                if (quantity <= 0) {
                    Console.WriteLine("Количество должно быть больше 0.");
                    return;
                }

                if (product.Stock >= quantity) {
                    if (items.ContainsKey(product))
                        items[product] += quantity;
                    else
                        items[product] = quantity;

                    product.Stock -= quantity;
                }
                else {
                    Console.WriteLine($"Недостаточно товара {product.Name} на складе. Доступно только {product.Stock} шт.");
                }
            }

            public void RemoveProduct(Product product, int quantity = 1) {
                if (items.ContainsKey(product)) {
                    if (quantity <= 0) {
                        Console.WriteLine("Количество должно быть больше 0.");
                        return;
                    }

                    items[product] -= quantity;
                    if (items[product] <= 0) items.Remove(product);
                    product.Stock += quantity;
                    Console.WriteLine($"{quantity} шт. товара {product.Name} удалено из корзины.");
                }
                else {
                    Console.WriteLine("Товара нет в корзине.");
                }
            }

            public void ShowCart() {
                if (items.Count == 0) {
                    Console.WriteLine("Корзина пуста.");
                    return;
                }

                Console.WriteLine("Корзина:");
                foreach (var item in items) {
                    decimal totalPrice = item.Key.Price * item.Value;
                    Console.WriteLine($"{item.Key.Name} x {item.Value} - {totalPrice} руб");
                }
                Console.WriteLine($"Итоговая сумма: {TotalPrice()}");
            }


            public decimal TotalPrice() {
                return items.Sum(item => item.Key.Price * item.Value);
            }

            public Dictionary<Product, int> Checkout() {
                return new Dictionary<Product, int>(items);
            }

            public void Clear() {
                items.Clear();
            }

            public Dictionary<Product, int> GetItems() {
                return items;
            }
        }

        class PurchaseHistory {
            private List<(List<Product>, decimal)> history = new List<(List<Product>, decimal)>();

            public void AddPurchase(List<Product> products, decimal totalPrice) {
                history.Add((products, totalPrice));
            }

            public void ShowHistory() {
                if (history.Count == 0) {
                    Console.WriteLine("История покупок пуста.");
                }
                else {
                    Console.WriteLine("История покупок:");
                    foreach (var purchase in history) {
                        Console.WriteLine("Товары: ");
                        foreach (var product in purchase.Item1) {
                            Console.WriteLine($"{product.Name} - {product.Price} руб");
                        }
                        Console.WriteLine($"Итоговая стоимость: {purchase.Item2} руб");
                    }
                }
                Console.WriteLine("Нажмите любую клавишу, чтобы вернуться назад.");
                Console.ReadKey(true);
            }
        }


        static void Main(string[] args) {
            var storeProducts = new List<Product>
            {
            new Product("Арбуз", "Вкусно", 49, "Вах вкусный арбуз бери не пожалеешь", 10),
            new Product("Дыня", "Вкусно", 89, "Супер дынька торпеда вах бабах", 20),
            new Product("Черешня", "Вкусно", 69, "Черешня вкусно бери налетай", 999),
            new Product("Слива", "Невкусно", 12, "Фигня в этом году, в другой раз нормана будет", 123),
        };

            var cart = new Cart();
            var history = new PurchaseHistory();

            var mainMenuItems = new List<string>
            {
            "Все товары",
            "Товары по категориям",
            "Корзина",
            "Оформить заказ",
            "История покупок"
        };

            bool exit = false;
            while (!exit) {
                NavigateMenu(mainMenuItems, selectedItem =>
                {
                    switch (selectedItem) {
                        case 0:
                            BrowseProducts(storeProducts, cart);
                            break;
                        case 1:
                            FilterByCategory(storeProducts, cart);
                            break;
                        case 2:
                            BrowseCart(cart);
                            break;
                        case 3:
                            PlaceOrder(cart, history);
                            break;
                        case 4:
                            history.ShowHistory();
                            break;
                        case 5:
                            exit = true;
                            break;
                    }
                });
            }
        }

        static void NavigateMenu(List<string> menuItems, Action<int> onSelect) {
            int selectedIndex = 0;
            bool itemSelected = false;

            while (!itemSelected) {
                Console.Clear();
                int maxLength = menuItems.Select(item => item.Length).Max();
                int borderWidth = maxLength + 4;
                Console.WriteLine("╔" + new string('═', borderWidth) + "╗");

                for (int i = 0; i < menuItems.Count; i++) {
                    if (i == selectedIndex)
                        Console.WriteLine($"║ > {menuItems[i].PadRight(maxLength)} ║");
                    else
                        Console.WriteLine($"║   {menuItems[i].PadRight(maxLength)} ║");
                }
                Console.WriteLine("╚" + new string('═', borderWidth) + "╝");

                var key = Console.ReadKey(true).Key;
                switch (key) {
                    case ConsoleKey.UpArrow:
                        if (selectedIndex > 0) selectedIndex--;
                        break;
                    case ConsoleKey.DownArrow:
                        if (selectedIndex < menuItems.Count - 1) selectedIndex++;
                        break;
                    case ConsoleKey.Enter:
                        itemSelected = true;
                        onSelect(selectedIndex);
                        break;
                    case ConsoleKey.Backspace:
                        itemSelected = true;
                        break;
                }
            }
        }

        static void BrowseProducts(List<Product> products, Cart cart) {
            Console.Clear();
            Console.WriteLine("Список товаров:");

            var productList = products.Select(p => $"{p.Name} ({p.Stock} шт.) - {p.Price} руб").ToList();

            NavigateMenu(productList, selectedIndex =>
            {
                var selectedProduct = products[selectedIndex];
                ShowProductDetails(selectedProduct, cart);
            });
        }

        static void FilterByCategory(List<Product> products, Cart cart) {
            DrawBorder("Введите категорию:");
            var category = Console.ReadLine();
            var filteredProducts = products.Where(p => p.Category == category).ToList();
            if (filteredProducts.Any()) {
                BrowseProducts(filteredProducts, cart);
            }
            else {
                DrawBorder("Нет товаров в этой категории.");
                Console.ReadKey(true);
            }
        }

        static void BrowseCart(Cart cart) {
            var cartItems = cart.GetItems().Keys.ToList();

            if (cartItems.Count == 0) {
                Console.Clear();
                Console.WriteLine("Корзина пуста.");
                Console.WriteLine("Нажмите любую клавишу, чтобы вернуться назад.");
                Console.ReadKey(true);
                return;
            }

            var cartProductList = cartItems.Select(item => $"{item.Name} (x{cart.GetItems()[item]}) - {cart.GetItems()[item] * item.Price} руб").ToList();

            NavigateMenu(cartProductList, selectedIndex =>
            {
                var selectedProduct = cartItems[selectedIndex];
                ShowCartProductDetails(selectedProduct, cart);
            });
        }

        static void DrawBorder(string content) {
            int maxLength = content.Length + 4;
            Console.WriteLine("╔" + new string('═', maxLength) + "╗");
            Console.WriteLine($"║ {content}   ║");
            Console.WriteLine("╚" + new string('═', maxLength) + "╝");
        }

        static void ShowProductDetails(Product product, Cart cart) {
            bool exit = false;
            while (!exit) {
                Console.Clear();
                var descriptionLines = product.Description.Split(new[] { '\n' }, StringSplitOptions.None);
                int maxLength = Math.Max(product.Name.Length, product.Category.Length + 10);
                foreach (var line in descriptionLines) {
                    maxLength = Math.Max(maxLength, line.Length);
                }

                maxLength += 4;
                Console.WriteLine("╔" + new string('═', maxLength) + "╗");
                Console.WriteLine($"║ {product.Name} ({product.Category}) - {product.Price} руб".PadRight(maxLength) + " ║");
                Console.WriteLine($"║ Описание: ".PadRight(maxLength) + " ║");

                foreach (var line in descriptionLines) {
                    Console.WriteLine($"║ {line.PadRight(maxLength - 2)} ║");
                }

                Console.WriteLine("╚" + new string('═', maxLength) + "╝");

                DrawBorder("Выберите действие:");
                Console.WriteLine("1.Добавить в корзину");
                Console.WriteLine("2.Вернуться назад");

                var key = Console.ReadKey(true).Key;
                switch (key) {
                    case ConsoleKey.D1:
                    case ConsoleKey.NumPad1:
                        DrawBorder("Введите количество:");
                        if (int.TryParse(Console.ReadLine(), out int quantity)) {
                            if (quantity > 0) {
                                if (quantity <= product.Stock) {
                                    cart.AddProduct(product, quantity);
                                    DrawBorder($"{quantity} шт. товара {product.Name} добавлено в корзину.");
                                }
                                else {
                                    DrawBorder($"Недостаточно товара на складе. Доступно: {product.Stock} шт.");
                                }
                            }
                            else {
                                DrawBorder("Количество должно быть больше 0.");
                            }
                        }
                        else {
                            DrawBorder("Некорректное количество.");
                        }
                        Console.WriteLine("Нажмите любую клавишу, чтобы продолжить...");
                        Console.ReadKey(true);
                        break;

                    case ConsoleKey.D2:
                    case ConsoleKey.NumPad2:
                    case ConsoleKey.Backspace:
                        exit = true;
                        break;
                }
            }
        }

        static void ShowCartProductDetails(Product product, Cart cart) {
            bool exit = false;
            while (!exit) {
                Console.Clear();

                int quantityInCart = cart.GetItems().ContainsKey(product) ? cart.GetItems()[product] : 0;

                int maxLength = Math.Max(product.Name.Length, product.Category.Length + 10) + 4;
                maxLength = Math.Max(maxLength, $"Количество в корзине: {quantityInCart}".Length) + 4;
                maxLength = Math.Max(maxLength, $"Цена: {product.Price} руб".Length) + 4;

                Console.WriteLine("╔" + new string('═', maxLength) + "╗");
                Console.WriteLine($"║ {product.Name} ({product.Category}) - {product.Price} руб".PadRight(maxLength) + " ║");
                Console.WriteLine($"║ Количество в корзине: {quantityInCart}".PadRight(maxLength) + " ║");
                Console.WriteLine("╚" + new string('═', maxLength) + "╝");

                Console.WriteLine("\nВыберите действие:");
                Console.WriteLine("1. Удалить из корзины");
                Console.WriteLine("2. Вернуться назад");

                var key = Console.ReadKey(true).Key;
                switch (key) {
                    case ConsoleKey.D1:
                    case ConsoleKey.NumPad1:
                        Console.WriteLine("Введите количество для удаления:");
                        if (int.TryParse(Console.ReadLine(), out int quantity) && quantity > 0) {
                            cart.RemoveProduct(product, quantity);
                        }
                        else {
                            Console.WriteLine("Некорректное количество.");
                        }
                        break;
                    case ConsoleKey.D2:
                    case ConsoleKey.NumPad2:
                    case ConsoleKey.Backspace:
                        exit = true;
                        break;
                }
            }
        }
        static void PlaceOrder(Cart cart, PurchaseHistory history) {
            cart.ShowCart();
            DrawBorder("Подтвердить покупку? (да/нет)");
            var confirm = Console.ReadLine().ToLower();
            if (confirm == "да") {
                var purchasedItems = cart.Checkout();
                history.AddPurchase(purchasedItems.Keys.ToList(), cart.TotalPrice());
                cart.Clear();
                DrawBorder("Покупка завершена.");
            }
            else {
                DrawBorder("Покупка отменена.");
            }
            Console.ReadKey(true);
        }


    }
}