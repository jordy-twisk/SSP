import pytest
import requests
import os
import api_info as a
from cerberus import Validator

def test_get_coaches():
  r = requests.get(a.api_link() + "profile/coach")
  assert r.status_code == 200, r.status_code
  v = Validator(a.s_coach())
  assert v.validate(r.json()[0]['coach']) == True, v.errors
  v = Validator(a.s_studentData())
  assert v.validate(r.json()[0]['user']) == True, v.errors
def test_post_coach():
  a.delete_coach()
  payload = "{\n    \"coach\": {\n      \"studentID\": \""+ a.coachId() + "\",\n      \"workload\": 10\n    },\n  \n    \"user\": {\n      \"studentID\": \""+ a.coachId() + "\",\n      \"firstName\": \"TestCoach\",\n      \"surName\": \"Test\",\n      \"phoneNumber\": \"0692495724\",\n      \"interests\": \"Programming (C only), Servers, Cisco\",\n      \"photo\": \"https://i.imgur.com/Tl5sYD6.jpg\",\n      \"description\": \"I am a coach\",\n      \"degree\": \"HBO\",\n      \"study\": \"Technische Informatica\",\n      \"studyYear\": 4\n    }}"  
  headers = {
    'Content-Type': "application/json",
    'cache-control': "no-cache"}
  r = requests.post(a.api_link() + "profile/coach", data=payload, headers=headers)
  if r.status_code is not 201:
    a.create_coach()
  assert r.status_code == 201, r.status_code
def test_get_coachProfile_byId():
  r = requests.get(a.api_link() + "profile/coach/" + a.coachId())
  assert r.status_code == 200, r.status_code
def test_delete_coachProfile_byId():
  r = requests.delete(a.api_link() + "profile/coach/" + a.coachId())
  if r.status_code is not 204:
    a.create_coach()
  assert r.status_code == 204, r.status_code
  a.create_coach()
def test_get_coach_byId():
  r = requests.get(a.api_link() + "coach/" + a.coachId())
  assert r.status_code == 200, r.status_code
def test_put_coach_byId():
  payload = "{\n\t\"studentID\": 000701,\n\t\"workload\": 5 \n}"
  headers = {
    'Content-Type': "application/json",
    'cache-control': "no-cache"}
  r = requests.put(a.api_link() + "coach/" + a.coachId(), data=payload, headers=headers)
  assert r.status_code == 204, r.text
