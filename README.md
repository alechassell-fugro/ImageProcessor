# Getting rotating an image to work
This was a nightmare, and one that took course over several distinct phases.

## 1. Flippin'
The easy part -- I copied the initial bitmapSource's bytes directly over to a new bitmapSource, with the width and height inverted. This obviously shredded the image to pieces because none of the mapping was implemented, and it was just writing the rows out on a mismatched output canvas.

## 2. Mappin'
The beginning of difficulties. I tried various methods of mapping from original to flipped. Initially I tried mapping index 0 to index 0, because for some reason I thought that would 'rotate', but after some very confusing outputs, Amy very helpfully pointed out that that was just not how rotation works, I would have to map index 0 to index end of first row.

## 3. Pain
Here I tried lots of things to get the mapping to go to the right spot. Something helpful I did was that I used an 100x100 image to test the rotation. After working out a formula on paper, for initially placing pixels at the end of the row, a helpful characterisation of flipping occured to me:
> __go from row r and col c to row c and col r__. 

This was really helpful because, when dealing with my scalar array, I had recorded the current row, and reused a column equation from a previous row, and so the problem became just writing index i to index rowLen * (c+1) - colLen * r. 

## 4. Border Problems
When the above was completed, the effect was so close to working. It was almmmmost perfect. One problem was a black pixel appearing in the top left corner. I tried shifting everything to the left by 4 bytes, because i figured maybe everything just got shifted, but this wasnt the problem. In fact doing that removed the black square in the top left, but created one in the bottom right. I tried debug printing the specific mappings, and it was very bizarre, because I could see that the initial mapping was in fact 4 off, and the later mapping was accurate, but they used the same logic, so I was at a loss! In retrospect, I should have noted that the equations were the same, and every other pixel looked right, so I should've made sure the variables in that equation were assuredly correct. The actual problem was that I incremented the row (which was used to decide how far across to put the output pixel) *after* the intial pixel being drawn, and so the value used for that equation was wrong, purely for the first pixel. I now have more motivation to check exactly the order of things occuring in my loops, and I should've kept an eye on whether or not that row value should've been incremented before or after the operation. 

This explicitly solved the weird nature of the black pixel not spreading to the other places, because the place it was hitting was then hit by another pixel. I'm sure I could delve further into why + how it happened but at this point it's 5:10pm and im quite tired. **maybe TODO: come up with a better explanation of why the black tile didn't spread, and also an explanation for the intuition of why it would've in the first place**

Finally, the most helpful thing was creating an image with characteristic borders, which showed very quickly the actual behaviour of the rotate.


## 5. Eternal Sunshine!