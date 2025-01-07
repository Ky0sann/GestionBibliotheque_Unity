using System.Collections.Generic;
using UnityEngine;  // Importation de la bibliothèque UnityEngine
using System.IO;
using Newtonsoft.Json; // Importation de la bibliothèque NewtonSoft.Json (remplace System.Text.Json)
using TMPro; // Importation de la bibliothèque TMPro (pour pouvoir gérer les éléments de l'UI en type Text Mesh Pro
using System.Text.RegularExpressions;
using System;

public class BibliothequeManager : MonoBehaviour // Déclaration de la classe BibliothequeManager en MonoBehaviour (nécessaire pour fonctionner dans Unity) 
{
    [System.Serializable] // Indique que une classe peut-être sérialisé (ici en JSON)
    public class Livre
    {
        public string titre;
        public string auteur;
        public int annee;
        public string genre;
        public string editeur;
        public string isbn;
        public string resume;

        public Livre(string titre, string auteur, int annee, string genre, string editeur, string isbn, string resume)
        {
            this.titre = titre;
            this.auteur = auteur;
            this.annee = annee;
            this.genre = genre;
            this.editeur = editeur;
            this.isbn = isbn;
            this.resume = resume;
        }

        public string Afficher()
        {
            // A la place de Console.WriteLine on a un return pour pouvoir afficher au format texte les livres (donc nécessaire de passer le type de la méthode en string et non void)
            return $"Titre: {titre}, Auteur: {auteur}, Année: {annee}, Genre: {genre}, Éditeur: {editeur}, ISBN: {isbn}, Résumé: {resume}";
        }
    }

    public class Bibliotheque
    {
        public List<Livre> livres = new List<Livre>();

        public bool VerifierIsbn(string isbn)
        {
            return livres.Exists(l => l.isbn == isbn);
        }

        public bool ValiderIsbn(string isbn)
        {
            string pattern = @"^\d{3}-\d{1,5}-\d{1,7}-\d{1,7}-\d{1}$";
            return Regex.IsMatch(isbn, pattern);
        }

        public bool VerifierAnnee(int annee)
        {
            int anneeactuelle = DateTime.Now.Year;
            if (annee > anneeactuelle)
            {
                return false;
            }
            return true;
        }

        public void AjouterLivre(Livre livre)
        {
            if (VerifierIsbn(livre.isbn))
            {
                // Unity possède aussi une console (comme un terminal sur vscode) et ce n'est pas Console.WriteLine mais Debug.Log (ainsi le type de la méthode peut rester en void)
                Debug.Log("L'ISBN existe déjà dans la bibliothèque. Impossible d'ajouter ce livre.\n");
                return;

            }

            if (!ValiderIsbn(livre.isbn))
            {
                // Utilisation de Debug.Log pour afficher un message dans la console de Unity 
                Debug.Log("L'ISBN n'est pas valide. Assurez-vous qu'il soit au bon format XXX-X-XX-XXXXX-X.\n");
                return;
            }

            if (!VerifierAnnee(livre.annee))
            {
                Debug.Log("Année invalide. L'année de publication ne peut pas être supérieure à l'année en cours.\n");
                return;
            }
            livres.Add(livre);
        }

        public string SupprimerLivre(string titre)
        {
            Livre livreASupprimer = livres.Find(l => l.titre.ToLower() == titre.ToLower());
            if (livreASupprimer != null)
            {
                livres.Remove(livreASupprimer);
                // On utilise return pour afficher le message en format text (donc type de la méthode passe en string)
                return $"Livre \"{titre}\" supprimé avec succès !";
            }
            return $"Livre \"{titre}\" introuvable.";
        }

        public List<Livre> RechercherLivre(string recherche)
        {
            return livres.FindAll(l =>
                l.titre.ToLower().Contains(recherche.ToLower()) ||
                l.auteur.ToLower().Contains(recherche.ToLower()) ||
                l.annee.ToString().Contains(recherche));
        }

        public string AfficherTout()
        {
            if (livres.Count > 0)
            {
                string result = "";
                foreach (var livre in livres)
                {
                    result += livre.Afficher() + "\n\n";
                }
                return result.Trim();
            }
            return "Aucun livre dans la bibliothèque.";
        }

        public void Sauvegarder(string cheminFichier)
        {
            try
            {
                // Sérialisation avec Newtonsoft.Json et non System.Text.Json
                string json = JsonConvert.SerializeObject(livres, Formatting.Indented);
                File.WriteAllText(cheminFichier, json);
                Debug.Log($"Bibliothèque sauvegardée avec succès : {cheminFichier}");
            }
            catch (System.Exception e)
            {
                // Debug possède sa propre méthode pour renvoyer une erreur : LogError
                Debug.LogError($"Erreur lors de la sauvegarde : {e.Message}");
            }
        }

        public void Charger(string cheminFichier)
        {
            try
            {
                if (File.Exists(cheminFichier))
                {
                    // Désérialisation avec Newtonsoft.Json et non System.Text.Json
                    string json = File.ReadAllText(cheminFichier);
                    livres = JsonConvert.DeserializeObject<List<Livre>>(json) ?? new List<Livre>();
                    Debug.Log("Bibliothèque chargée avec succès !");
                }
                else
                {
                    // Debug possède sa propre méthode pour envoyer un avertissement (différent d'une erreur) : LogWarning
                    Debug.LogWarning($"Le fichier {cheminFichier} n'existe pas.");
                }
            }
            catch (System.Exception e)
            {
                Debug.LogError($"Erreur lors du chargement : {e.Message}");
            }
        }
    }

    // Références Unity qui fait le lien entre le script et l'UI
    // TMP_InputField pour donner la référence des Input_Text créé dans la hiérarchie
    public TMP_InputField titreInput, auteurInput, anneeInput, genreInput, editeurInput, isbnInput, resumeInput;
    //TMP_Text pour donner la référence de la zone d'affichage (affichageText)
    public TMP_Text affichageText;

    // Attention ! La partie suivante s'apparente à la classe Program et à la méthode Main
    public Bibliotheque bibliotheque = new Bibliotheque();

    public void AjouterLivre() // Méthode AjouterLivre de BibliothequeManager qui appelle la méthode du même nom de la sous class Bibliothèque
    {
        try
        {
            Livre livre = new Livre( // Création du livre 
                titreInput.text, // Récupération de l'inputField de titre 
                auteurInput.text, // Récupération de l'inputField de auteur
                int.Parse(anneeInput.text), // Récupération de l'inputField de annee (convertit ensuite en int)
                genreInput.text, // Récupération de l'inputField de genre
                editeurInput.text, // Récupération de l'inputField de editeur
                isbnInput.text, // Récupération de l'inputField de isbn
                resumeInput.text // Récupération de l'inputField de resume
            );
            bibliotheque.AjouterLivre(livre); // On ajoute le livre en utilisant la méthode AjouterLivre de bibliotheque
            AfficherTout(); // On affiche tout les livres ensuite 
        }
        catch (System.Exception e)
        {
            Debug.LogError($"Erreur lors de l'ajout d'un livre : {e.Message}");
        }
    }

    public void SupprimerLivre()
    {
        string resultat = bibliotheque.SupprimerLivre(titreInput.text); // On appelle la méthode SupprimerLivre de la sous classe bibliotheque
        affichageText.text = resultat; // Dans la zone de texte, on affiche ensuite le livre qui vient d'être supprimé
        AfficherTout(); 
    }

    public void RechercherLivre()
    {
        var resultats = bibliotheque.RechercherLivre(titreInput.text); // On appelle la méthode RechercherLivre de la sous classe bibliotheque
        affichageText.text = ""; // Dans la zone de texte, on affiche d'abord un espace vide
        foreach (var livre in resultats) // Puis pour chaque livre correspondant au résultat
        {
            affichageText.text += livre.Afficher() + "\n"; // On ajoute dans la zone de texte le livre en question
        }
    }

    public void AfficherTout() 
    {
        affichageText.text = bibliotheque.AfficherTout(); // // On appelle la méthode AfficherTout de la sous classe bibliotheque
    }

    public void Sauvegarder()
    {
        string cheminFichier = Application.persistentDataPath + "/bibliotheque.json"; // On stocke le tout dans le fichier bibliotheque.Json
        bibliotheque.Sauvegarder(cheminFichier); // Pour se faire, on appelle la méthode Sauvegarder de la sous classe bibliotheque
    }

    public void Charger()
    {
        string cheminFichier = Application.persistentDataPath + "/bibliotheque.json"; // On lis le tout dans le fichier bibliotheque.Json
        bibliotheque.Charger(cheminFichier); // Pour se faire, on appelle la méthode Charger de la sous classe bibliotheque
        AfficherTout(); // On affiche tout les livres
    }
}
