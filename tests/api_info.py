def api_link():
  return "https://dev-tinderfunv2.azurewebsites.net/api/"
def studentId():
  return "581433"
def coachId():
  return "000701"

def create_coach():
  import requests
  url = api_link() + "profile/coach"
  payload = "{\n    \"user\": {\n      \"studentID\": \"000701\",\n      \"firstName\": \"TestCoach\",\n      \"surName\": \"Test\",\n      \"phoneNumber\": \"0692495724\",\n      \"interests\": \"Programming (C only), Servers, Cisco\",\n      \"photo\": \"https://i.imgur.com/Tl5sYD6.jpg\",\n      \"description\": \"My name is Thomas Jansema and I am a student\",\n      \"degree\": \"HBO\",\n      \"study\": \"Technische Informatica\",\n      \"studyYear\": 4\n    },\n    \"coach\": {\n      \"studentID\": \"000701\",\n      \"workload\": 10\n    }\n  }"  
  headers = {
    'Content-Type': "application/json",
    'cache-control': "no-cache"}
  response = requests.request("POST", url, data=payload, headers=headers)
def delete_coach():
  import requests
  r = requests.delete(api_link() + "profile/coach/" + coachId())
