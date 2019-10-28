import pytest
import requests
import os
import api_info as a

def test_get_coaches():
  r = requests.get(a.api_link() + "profile/coach")
  assert r.status_code == 200, r.status_code
def test_post_coaches():
  r = requests.post(a.api_link() + "profile/coach")
  assert r.status_code == 201, r.status_code
def test_get_coach_byId():
  r = requests.get(a.api_link() + "profile/coach/" + a.studentId())
  assert r.status_code == 200, r.status_code
