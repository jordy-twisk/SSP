import pytest
import requests
import os
import api_info as a

def test_get_student():
  print("test_get_student")
  r = requests.get(a.api_link() + "student/" + a.studentId())
  assert r.status_code == 200, r.status_code
def test_put_student():
  print("test_put_student")
  r = requests.get(a.api_link() + "student/" + a.studentId())
  assert r.status_code == 200, r.status_code
def test_get_student():
  print("test_get_student")
  r = requests.get(a.api_link() + "students/search?studentId=" + a.studentId())
  assert r.status_code == 200, r.status_code
