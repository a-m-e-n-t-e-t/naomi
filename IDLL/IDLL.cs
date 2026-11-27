using System;
using System.Collections.Generic;

namespace IDLL
{
    /// <summary>
    /// Контракт, который обязаны реализовать все плагины системы NAOMI.
    /// Плагин регистрируется в хост-форме через <see cref="I_Plugin_Hostform.Register"/>
    /// и получает пользовательские вызовы через <see cref="P_Proc"/>.
    /// </summary>
    public interface I_Plugin
    {
        /// <summary>Короткое уникальное имя плагина (используется как ключ меню).</summary>
        string P_Name { get; }

        /// <summary>Человекочитаемое описание назначения плагина.</summary>
        string P_Discript { get; }

        /// <summary>Версия плагина в формате Major.Minor.Build.</summary>
        string P_Version { get; }

        /// <summary>Тип возвращаемых данных: "text", "html", "image" и т. д.</summary>
        string P_Type { get; }

        /// <summary>
        /// Массив строк-описателей команд плагина.
        /// Формат каждой строки: «Имя|Действие|Подпись_меню|Параметр|».
        /// Хост превращает каждый элемент в пункт подменю плагина.
        /// </summary>
        string[] P_Integr { get; set; }

        /// <summary>Ссылка на хост-форму, установливается во время регистрации.</summary>
        I_Plugin_Hostform Host { get; set; }

        /// <summary>
        /// Точка входа для пользовательских команд.
        /// Параметр <paramref name="proc_param"/> — это конкатенация Tag пункта меню
        /// и текста верхнего RichTextBox: «index|Имя|Действие|Подпись|Параметр|Входной_текст».
        /// </summary>
        /// <returns>Строка-результат для отображения в нижнем RichTextBox (может быть пустой).</returns>
        string P_Proc(string proc_param);

        /// <summary>Вызывается хостом сразу после создания экземпляра плагина.</summary>
        void P_Ini();
    }

    /// <summary>
    /// Контракт хост-формы: даёт плагину возможность зарегистрировать себя.
    /// </summary>
    public interface I_Plugin_Hostform
    {
        /// <summary>
        /// Регистрирует плагин в хосте.
        /// </summary>
        /// <returns>true, если регистрация прошла успешно; иначе false.</returns>
        bool Register(I_Plugin plug);
    }
}
