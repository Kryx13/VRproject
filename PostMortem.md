# Post-Mortem 
# 🌿 Bonsai Therapy VR

### *Find your inner peace, one bug at a time.*

---

## 🚀 The Good Stuff: Learning from the Past

Honestly, the previous project (that Unity Video Game) was a lifesaver. It helped me dodge a lot of early conception issues and be way more efficient right out of the gate. I actually listened to my past self and made two big changes:

* **Ambition Check:** I kept the project way less ambitious this time to actually get it finished.
* **Asset Testing:** I actually took the time to test materials and Unity Store assets early on instead of just praying they’d work.

---

## 🌲 The Hardware Struggle (Accepting the "Lo-Fi" Life)

My computer power is... not great.

* **The Tree Incident:** I tried to make a realistic tree, but it was way too heavy for my system to handle.
* **The Solution:** I had to pivot to using cylinders and spheres. Is it realistic? No. Is it less heavy? Yes. We just had to accept the "abstract" look for the sake of performance.
* **Time Management:** Since I had very limited time with the actual headset, I didn't want to waste a single second debugging on the fly, so I chose stability over fancy visuals.

---

## 💻 The Lab & Version Hell

Since my own laptop isn't compatible with VR headsets, I was forced to move the project to the school’s info lab. That was a classic "Unity Moment":

* **Version Mismatch:** The lab computers weren't on the same Unity version as mine.
* **WIFI Speed:** Between the version issues and the slow WiFi, I managed to lose 2 hours just trying to get the project open. Good times.

---

## 🔍 Scaling & UX Tweaks

I ran into a weird resolution issue I didn't have last time. My laptop is set to **150% zoom** instead of the standard **100%**, which meant buttons and UI elements looked massive on other computers.

I also spent a chunk of time fighting with the **camera position**. Making sure the game was playable while **seated**—a classic VR hurdle—took a few trials, but I eventually got the height feeling right.

---

## 🛠️ The "Build Error" Final Boss

Just when I thought I was done, the build errors started screaming.

* **The "Vague" Error:** `Error building Player because scripts have compile errors in the editor`. Super helpful, Unity. Thanks.
* **The Prefab Culprit:** It turned out a prefab in the VR Template (`Complete XR Origin Set Up Hands Variant`) was corrupt or missing a parent variant.
* **Namespace Issues:** I also had `UnityEditor` namespaces in my scripts. Pro-tip: those are **not** compatible with Android builds, and they will break your build every single time.

---

**Would you like me to help you draft a "Best Practices" checklist for your current AR project based on these VR lessons?**