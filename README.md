## Video Poker Analyzer

###

```bash

// View the time it took to score
wget -O- http://localhost:5000/

// Try a few hands
wget -O- http://localhost:5000/analyze?cards=AH,2H,3H,4H,5H

wget -O- http://localhost:5000/analyze?cards=9D,TH,JH,QH,KH

wget -O- http://localhost:5000/analyze?cards=9H,TH,JH,QH,KH

wget -O- http://localhost:5000/analyze?cards=8H,TH,JH,QH,KH

```