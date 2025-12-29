/** @type {import('tailwindcss').Config} */
module.exports = {
    content: [
        "./Views/**/*.cshtml",
        "./Areas/**/*.cshtml",
        "./Pages/**/*.cshtml"
    ],
    theme: {
        fontFamily: {
            sans: ["Marianne", "sans-serif"],
        },
        extend: {
            colors: {
                // Texte
                textPrimary: "#3A3A3A",

                // Couleurs officielles
                primary: "#000091",      // Bleu France
                success: "#008941",      // Vert Marianne
                error: "#E1000F",        // Rouge Marianne

                // Surfaces
                surface: "#f5f5fe",      // Bleu-gris très pâle
            },
        },
    },
    plugins: [],
};