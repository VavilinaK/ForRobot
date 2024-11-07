import sys
from PyQt5.QtWidgets import (QApplication, QMainWindow, QWidget, 
                            QVBoxLayout, QPushButton, QLabel)
from PyQt5.QtCore import Qt

class MainWindow(QMainWindow):
    """Базовое главное окно"""
    
    def __init__(self):
        super().__init__()
        self.setup_ui()
        
    def setup_ui(self):
        """Настройка интерфейса"""
        # Основные параметры окна
        self.setWindowTitle("Базовое приложение")
        self.setGeometry(100, 100, 400, 300)  # x, y, width, height
        
        # Создаем центральный виджет и слой
        central_widget = QWidget()
        self.setCentralWidget(central_widget)
        layout = QVBoxLayout(central_widget)
        
        # Добавляем виджеты
        label = QLabel("Привет, PyQt5!")
        label.setAlignment(Qt.AlignCenter)
        layout.addWidget(label)
        
        button = QPushButton("Нажми меня")
        button.clicked.connect(self.button_clicked)
        layout.addWidget(button)
        
    def button_clicked(self):
        """Обработчик нажатия кнопки"""
        print("Кнопка нажата!")

def main():
    # Создаем приложение
    app = QApplication(sys.argv)
    
    # Создаем и показываем главное окно
    window = MainWindow()
    window.show()
    
    # Запускаем цикл обработки событий
    sys.exit(app.exec_())

if __name__ == '__main__':
    main()